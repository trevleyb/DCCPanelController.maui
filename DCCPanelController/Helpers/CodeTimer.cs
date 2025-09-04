using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Helpers;

public sealed class CodeTimer : IDisposable {
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private readonly string _name;
    private readonly TimeSpan _minLogThreshold;
    private readonly Action<string> _logger;

    // New parameter: minLogThreshold (default 0 -> always prints)
    public CodeTimer(string name, TimeSpan? minLogThreshold = null, Action<string>? logger = null) {
        _name = name;
        _minLogThreshold = minLogThreshold ?? TimeSpan.Zero;
        _logger = logger ?? Console.WriteLine;
    }

    // Convenience overload using milliseconds
    public CodeTimer(string name, double minLogMilliseconds, Action<string>? logger = null)
        : this(name, TimeSpan.FromMilliseconds(minLogMilliseconds), logger) { }

    public void Dispose() {
        _sw.Stop();
        if (_sw.Elapsed >= _minLogThreshold) {
            _logger($"{_name} took {_sw.Elapsed.TotalMilliseconds:N1} ms");
        }
    }
}