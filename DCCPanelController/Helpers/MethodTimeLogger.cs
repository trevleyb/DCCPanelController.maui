using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using DCCPanelController.Helpers.Logging;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Helpers;

public static class MethodTimeLogger {
    // ---- Public knobs ----------------------------------------------------
    public static string? LogDirectory { 
        get;
        set {
            field = value;
            Console.WriteLine($"TimerLogPath: {value}");
        }
    } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PerfLogs");

    public static string LogFileName { get; set; } = "method-timings.csv";
    public static long MaxBytesBeforeRotate { get; set; } = 10 * 1024 * 1024; // 10 MB
    public static bool IncludeHeaderOnNewFile { get; set; } = true;

    public static char CsvDelimiter { get; set; } = ',';                // or '\t' for TSV
    public static bool QuoteMessageField { get; set; } = false;         // <- your ask
    public static bool StripNewlinesInMessage { get; set; } = true;     // keep rows single-line
    public static string? MessageCommaReplacement { get; set; } = null; // e.g. ";" if keeping delimiter=','
    
    // ---- Public API called by MethodTimer.Fody ---------------------------
    public static void Log(MethodBase method, long milliseconds, string? message) {
        #if PERF_TIMING

        // Original console/debug output (keep if you like)
        Write($"{Pretty(method)}  {milliseconds,7} ms{Suffix(message)}");

        // CSV line enqueue
        CsvLogger.Enqueue(
            DateTimeOffset.UtcNow,
            milliseconds,
            Environment.CurrentManagedThreadId,
            method,
            message
        );
        #endif
    }

    // Overloads some MethodTimer versions may use:
    public static void Log(MethodBase method, TimeSpan elapsed, string? message) => Log(method, (long)elapsed.TotalMilliseconds, message);

    // ---- Helpers ---------------------------------------------------------
    private static string Pretty(MethodBase m) => $"{m.DeclaringType?.FullName}.{m.Name}";
    private static string Suffix(string? msg) => string.IsNullOrWhiteSpace(msg) ? "" : $"  | {msg}";

    private static void Write(string line) {
        LogHelper.Logger.LogInformation(line);
    }

    // ---- CSV Writer (internal) ------------------------------------------
    private static class CsvLogger {
        private static readonly ConcurrentQueue<string> _q    = new();
        private static readonly SemaphoreSlim           _gate = new(1, 1);
        private static          StreamWriter?           _writer;
        private static          string?                 _currentPath;
        private static          long                    _bytes;
        private static volatile bool                    _started;
        private static readonly CancellationTokenSource _cts = new();
        private static          Task?                   _pumpTask;

       public static void Enqueue(DateTimeOffset ts, long ms, int threadId, MethodBase method, string? message)
        {
            EnsureStarted();

            // Prepare message according to your preference
            var msg = message ?? string.Empty;
            if (StripNewlinesInMessage)
                msg = msg.Replace('\r', ' ').Replace('\n', ' ');

            if (!QuoteMessageField && MessageCommaReplacement is not null && CsvDelimiter == ',')
                msg = msg.Replace(",", MessageCommaReplacement);

            // Build the record: quote all the non-message fields; message handled separately
            var rec = string.Join(CsvDelimiter, new[]
            {
                Csv(ts.ToString("o", System.Globalization.CultureInfo.InvariantCulture)), // ISO 8601
                Csv(ms.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                Csv(threadId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                Csv(MethodKind(method)),
                Csv(method.Name),
                Csv(method.DeclaringType?.FullName ?? string.Empty),
                QuoteMessageField ? Csv(msg) : CsvRaw(msg) // <--- key difference
            });

            _q.Enqueue(rec);
        }

        private static string Csv(string value)
        {
            // Standard CSV escaping for non-message fields
            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 || value.IndexOf(CsvDelimiter) >= 0)
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static string CsvRaw(string value)
        {
            // No quoting at all; just return raw text
            // (We already stripped CR/LF above if configured; delimiter may still be present)
            return value;
        }

        private static string MethodKind(MethodBase m)
        {
            if (m.IsConstructor) return "ctor";
            if (m.IsStatic) return "static";
            return "instance";
        }
        
        private static void EnsureStarted() {
            if (_started) return;
            lock (_q) {
                if (_started) return;
                _started = true;
                _pumpTask = Task.Run(PumpAsync);
            }
        }

        private static async Task PumpAsync() {
            try {
                await EnsureWriterAsync().ConfigureAwait(false);
                var token = _cts.Token;

                while (!token.IsCancellationRequested) {
                    // Batch up to reduce I/O syscalls
                    if (_q.IsEmpty) {
                        await Task.Delay(25, token).ConfigureAwait(false);
                        continue;
                    }

                    // Drain a chunk
                    for (int i = 0; i < 512 && _q.TryDequeue(out var line); i++) {
                        await RotateIfNeededAsync().ConfigureAwait(false);
                        await WriteLineAsync(line).ConfigureAwait(false);
                    }

                    await _writer!.FlushAsync().ConfigureAwait(false);
                }
            } catch (OperationCanceledException) { /* normal on shutdown */
            } catch (Exception ex) {
                LogHelper.Logger.LogWarning($"CsvLogger error: {ex}");
            } finally {
                try {
                    _writer?.Flush();
                    _writer?.Dispose();
                } catch { /* ignore */
                }
            }
        }

        private static async Task EnsureWriterAsync() {
            await _gate.WaitAsync().ConfigureAwait(false);
            try {
                if (_writer != null) return;

                Directory.CreateDirectory(MethodTimeLogger.LogDirectory!);
                _currentPath = Path.Combine(MethodTimeLogger.LogDirectory!, MethodTimeLogger.LogFileName);
                var newFile = !File.Exists(_currentPath);

                _writer = new StreamWriter(new FileStream(
                        _currentPath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.Read),
                    System.Text.Encoding.UTF8);

                _bytes = new FileInfo(_currentPath).Length;

                if (IncludeHeaderOnNewFile && newFile) {
                    var header = "timestamp,ms,threadId,kind,method,declaringType,message";
                    await WriteLineAsync(header).ConfigureAwait(false);
                }
            } finally {
                _gate.Release();
            }
        }

        private static async Task RotateIfNeededAsync() {
            if (_bytes < MaxBytesBeforeRotate) return;

            await _gate.WaitAsync().ConfigureAwait(false);
            try {
                if (_bytes < MaxBytesBeforeRotate) return; // double-check inside lock

                _writer?.Flush();
                _writer?.Dispose();

                var ts = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                var rotated = Path.Combine(
                    Path.GetDirectoryName(_currentPath!)!,
                    Path.GetFileNameWithoutExtension(_currentPath!) + $"_{ts}" + Path.GetExtension(_currentPath!)
                );
                File.Move(_currentPath!, rotated, overwrite: false);

                // Re-create
                _writer = new StreamWriter(new FileStream(
                        _currentPath!,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.Read),
                    System.Text.Encoding.UTF8);
                _bytes = 0;

                if (IncludeHeaderOnNewFile) {
                    var header = "timestamp,ms,threadId,kind,method,declaringType,message";
                    await WriteLineAsync(header).ConfigureAwait(false);
                }
            } finally {
                _gate.Release();
            }
        }

        private static async Task WriteLineAsync(string line) {
            await _writer!.WriteLineAsync(line).ConfigureAwait(false);
            _bytes += (line.Length + Environment.NewLine.Length); // rough count, fine for rotation
        }
        
        // Optional: call on app shutdown if you want
        public static async Task FlushAndStopAsync() {
            await _cts.CancelAsync();
            if (_pumpTask != null) {
                try {
                    await _pumpTask.ConfigureAwait(false);
                } catch { /* ignore */
                }
            }
        }
    }
}