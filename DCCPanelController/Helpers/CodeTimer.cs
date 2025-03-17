namespace DCCPanelController.Helpers;

using System.Diagnostics;

public class CodeTimer : IDisposable {

    private static int _level;
    private readonly Stopwatch _stopwatch;
    private readonly string _blockName;

    public CodeTimer(string blockName) {
#if DEBUG
        _blockName = blockName;
        _level++;
        _stopwatch = Stopwatch.StartNew();
        
#endif
    }
    
    public void Dispose() {
#if DEBUG
        _stopwatch.Stop();
        Console.WriteLine($"{new string('\t', _level)}[{_blockName}] executed in {_stopwatch.ElapsedMilliseconds}ms");
        _level--;
#endif
    }
}