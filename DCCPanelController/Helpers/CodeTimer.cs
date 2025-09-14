using System.Diagnostics;

namespace DCCPanelController.Helpers;

public sealed class CodeTimer : IDisposable {
    private readonly Action<string> _logger;
    private readonly TimeSpan       _minLogThreshold;
    private readonly string         _name;
    private readonly bool           _output = true;
    private readonly Stopwatch      _sw     = Stopwatch.StartNew();

    // New parameter: minLogThreshold (default 0 -> always prints)
    public CodeTimer(string name, bool output = true, TimeSpan? minLogThreshold = null, Action<string>? logger = null) {
        _name = name;
        _minLogThreshold = minLogThreshold ?? TimeSpan.Zero;
        _logger = logger ?? Console.WriteLine;
        _output = output;
    }

    // Convenience overloads
    public CodeTimer(string name) : this(name, true, TimeSpan.FromMilliseconds(0)) { }
    public CodeTimer(string name, bool output) : this(name, output, TimeSpan.FromMilliseconds(0)) { }
    public CodeTimer(string name, Action<string>? logger = null) : this(name, true, TimeSpan.FromMilliseconds(0), logger) { }
    public CodeTimer(string name, bool output, Action<string>? logger = null) : this(name, output, TimeSpan.FromMilliseconds(0), logger) { }
    public CodeTimer(string name, double minLogMilliseconds, Action<string>? logger = null) : this(name, true, TimeSpan.FromMilliseconds(minLogMilliseconds), logger) { }
    public CodeTimer(string name, bool output, double minLogMilliseconds, Action<string>? logger = null) : this(name, output, TimeSpan.FromMilliseconds(minLogMilliseconds), logger) { }

    public void Dispose() {
        _sw.Stop();
        if (_sw.Elapsed >= _minLogThreshold && _output) {
            _logger($"{_name} took {_sw.Elapsed.TotalMilliseconds:N1} ms");
        }
    }
}