namespace DCCPanelController.Helpers;

using System.Diagnostics;

public class CodeTimer : IDisposable {
    private readonly Stopwatch _stopwatch;
    private readonly string _blockName;

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