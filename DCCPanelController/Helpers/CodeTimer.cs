using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Helpers;

public class CodeTimer : IDisposable {
    private readonly string _blockName;
    private readonly Stopwatch _stopwatch;
    private readonly bool _writeOutput;

    public CodeTimer(string blockName, bool writeOutput = true) {
#if DEBUG
        _blockName = blockName;
        _stopwatch = Stopwatch.StartNew();
        _writeOutput = writeOutput;

#endif
    }

    public void Dispose() {
#if DEBUG
        _stopwatch.Stop();
        var logger = LogHelper.CreateLogger("CodeTimer");
        if (_writeOutput) {
            logger.LogDebug("[{BlockName}] executed in {StopwatchElapsedMilliseconds}ms", _blockName, _stopwatch.ElapsedMilliseconds);
        }
#endif
    }
}