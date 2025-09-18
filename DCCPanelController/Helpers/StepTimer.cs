using System.Diagnostics;

namespace DCCPanelController.Helpers;

using System;
using System.Diagnostics;

public static class StepTimer {
    private static readonly object     _lock = new();
    private static          Stopwatch? _stopwatch;
    private static          TimeSpan   _lastStep;
    private static          string     _name = "StepTimer";

    /// <summary>
    /// Start or reset the timer.
    /// </summary>
    public static void Start(string? name = null) {
        lock (_lock) {
            _name = name ?? "StepTimer";
            _stopwatch = Stopwatch.StartNew();
            _lastStep = TimeSpan.Zero;
            Console.WriteLine($"[{_name}] Started");
        }
    }

    /// <summary>
    /// Record a step with a description.
    /// Outputs time since last step and total elapsed time.
    /// </summary>
    public static void Step(string description) {
        lock (_lock) {
            if (_stopwatch is null) {
                Console.WriteLine($"[{_name}] Step called before Start: {description}");
                return;
            }

            var elapsed = _stopwatch.Elapsed;
            var sinceLast = elapsed - _lastStep;
            _lastStep = elapsed;

            Console.WriteLine($"[{_name}] {description} | Step: {sinceLast.TotalMilliseconds:N1} ms | Total: {elapsed.TotalMilliseconds:N1} ms");
        }
    }

    /// <summary>
    /// Finish timing and output total duration.
    /// </summary>
    public static void Finish() {
        lock (_lock) {
            if (_stopwatch is null) {
                Console.WriteLine($"[{_name}] Finish called before Start");
                return;
            }

            var elapsed = _stopwatch.Elapsed;
            Console.WriteLine($"[{_name}] Finished | Total duration: {elapsed.TotalMilliseconds:N1} ms");
            _stopwatch = null;
        }
    }
}