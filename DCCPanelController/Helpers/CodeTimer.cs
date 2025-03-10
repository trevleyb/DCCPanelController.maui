namespace DCCPanelController.Helpers;

using System.Diagnostics;

public class CodeTimer(string blockName = "Code Block") : IDisposable {
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    public void Dispose() {
        _stopwatch.Stop();
        Console.WriteLine($"[{blockName}] executed in {_stopwatch.ElapsedMilliseconds}ms");
    }
}