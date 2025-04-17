using System.Diagnostics;

namespace DCCPanelController.Helpers;

public class CodeTimer : IDisposable {
    private readonly string _blockName;
    private readonly Stopwatch _stopwatch;

    public CodeTimer(string blockName) {
#if DEBUG
        _blockName = blockName;
        _stopwatch = Stopwatch.StartNew();

#endif
    }

    public void Dispose() {
#if DEBUG
        _stopwatch.Stop();
        Console.WriteLine($"[{_blockName}] executed in {_stopwatch.ElapsedMilliseconds}ms");
#endif
    }
}