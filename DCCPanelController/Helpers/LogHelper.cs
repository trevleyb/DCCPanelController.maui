using Microsoft.Extensions.Logging;

namespace DCCPanelController.Helpers;

public static class LogHelper {
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;

    public static void Initialize(ILoggerFactory loggerFactory) {
        if (_loggerFactory != null)
            throw new InvalidOperationException("LogHelper has already been initialized.");

        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger("GlobalLogger");
    }

    public static ILogger Logger {
        get {
            if (_logger == null)
                throw new InvalidOperationException("LogHelper has not been initialized. Call LogHelper.Initialize() first.");

            return _logger;
        }
    }

    public static ILogger CreateLogger<T>() => _loggerFactory?.CreateLogger<T>() ?? throw new InvalidOperationException("LogHelper has not been initialized.");
}