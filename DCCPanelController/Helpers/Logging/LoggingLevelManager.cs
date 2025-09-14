using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DCCPanelController.Helpers;

public static class LoggingLevelHelper {
    private static LoggingLevelSwitch? _levelSwitch;

    /// <summary>
    ///     Check if the helper has been initialized
    /// </summary>
    public static bool IsInitialized => _levelSwitch != null;

    /// <summary>
    ///     Initialize the logging level switch. Call this during Serilog configuration.
    /// </summary>
    /// <param name="initialLevel">The initial logging level</param>
    /// <returns>The LoggingLevelSwitch to use in Serilog configuration</returns>
    public static LoggingLevelSwitch Initialize(LogEventLevel initialLevel = LogEventLevel.Debug) {
        _levelSwitch = new LoggingLevelSwitch(initialLevel);
        return _levelSwitch;
    }

    /// <summary>
    ///     Change the logging level at runtime
    /// </summary>
    /// <param name="level">The new logging level</param>
    public static void SetLogLevel(LogEventLevel level) {
        if (_levelSwitch == null) {
            throw new InvalidOperationException("LoggingLevelHelper has not been initialized. Call Initialize() during Serilog setup.");
        }

        _levelSwitch.MinimumLevel = level;
        Log.Information("Logging level changed to {Level}", level);
    }

    /// <summary>
    ///     Change the logging level using Microsoft.Extensions.Logging LogLevel enum
    /// </summary>
    /// <param name="level">The Microsoft LogLevel</param>
    public static void SetLogLevel(LogLevel level) {
        var serilogLevel = ConvertToSerilogLevel(level);
        SetLogLevel(serilogLevel);
    }

    /// <summary>
    ///     Change the logging level using a string value
    /// </summary>
    /// <param name="levelString">The logging level as a string (Debug, Information, Warning, Error, Fatal)</param>
    public static void SetLogLevel(string levelString) => SetLogLevel(Enum.TryParse<LogEventLevel>(levelString, true, out var level) ? level : LogEventLevel.Debug);

    /// <summary>
    ///     Get the current logging level
    /// </summary>
    /// <returns>The current LogEventLevel</returns>
    public static LogEventLevel GetCurrentLogLevel() {
        if (_levelSwitch == null) {
            throw new InvalidOperationException("LoggingLevelHelper has not been initialized.");
        }

        return _levelSwitch.MinimumLevel;
    }

    /// <summary>
    ///     Convert Microsoft.Extensions.Logging.LogLevel to Serilog LogEventLevel
    /// </summary>
    private static LogEventLevel ConvertToSerilogLevel(LogLevel logLevel) => logLevel switch {
        LogLevel.Trace       => LogEventLevel.Verbose,
        LogLevel.Debug       => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning     => LogEventLevel.Warning,
        LogLevel.Error       => LogEventLevel.Error,
        LogLevel.Critical    => LogEventLevel.Fatal,
        LogLevel.None        => LogEventLevel.Fatal,
        _                    => LogEventLevel.Information,
    };
}