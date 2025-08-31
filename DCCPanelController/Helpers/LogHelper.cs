using Microsoft.Extensions.Logging;

namespace DCCPanelController.Helpers;

public static class LogHelper {
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;

    public static ILogger Logger {
        get {
            if (_logger == null)
                throw new InvalidOperationException("LogHelper has not been initialized. Call LogHelper.Initialize() first.");
            return _logger;
        }
    }

    public static void Initialize(ILoggerFactory loggerFactory) {
        if (_loggerFactory != null)
            throw new InvalidOperationException("LogHelper has already been initialized.");

        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger("GlobalLogger");
    }

    // Add this method to create typed loggers easily
    public static ILogger<T> CreateLogger<T>() {
        return _loggerFactory?.CreateLogger<T>() ??
               throw new InvalidOperationException("LogHelper has not been initialized.");
    }

    // Add this method for runtime type creation
    public static ILogger CreateLogger(Type type) {
        return _loggerFactory?.CreateLogger(type) ??
               throw new InvalidOperationException("LogHelper has not been initialized.");
    }

    // Add this method for string-based categories
    public static ILogger CreateLogger(string categoryName) {
        return _loggerFactory?.CreateLogger(categoryName) ??
               throw new InvalidOperationException("LogHelper has not been initialized.");
    }

    public static string GetLogDirectory() {
        var logDirectory = "";

#if IOS || MACCATALYST
        logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Logs");
#elif ANDROID
        logDirectory = Path.Combine(FileSystem.Current.AppDataDirectory, "Logs");
#elif WINDOWS
        logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DCCPanelController", "Logs");
#else
        logDirectory = Path.Combine(FileSystem.Current.AppDataDirectory, "Logs");
#endif
        Directory.CreateDirectory(logDirectory);
        return Path.Combine(logDirectory, "dccpanelcontroller.log");
    }

    public static async Task<string[]> GetLogFilesAsync() {
        var logDirectory = GetLogDirectory();
        if (!Directory.Exists(logDirectory)) return [];

        return Directory.GetFiles(logDirectory, "*.log")
                        .OrderByDescending(File.GetCreationTime)
                        .ToArray();
    }

    public static async Task<string> GetLatestLogContentAsync() {
        var logFiles = await GetLogFilesAsync();
        if (logFiles.Length == 0) return "No log files found.";

        try {
            return await File.ReadAllTextAsync(logFiles[0]);
        } catch (Exception ex) {
            return $"Error reading log file: {ex.Message}";
        }
    }

    public static async Task ShareLogFileAsync() {
        var logFiles = await GetLogFilesAsync();
        if (logFiles.Length == 0) return;

        try {
            await Share.Default.RequestAsync(new ShareFileRequest {
                Title = "Share Log File",
                File = new ShareFile(logFiles[0])
            });
        } catch (Exception ex) {
            throw new ApplicationException("Failed to share log file", ex);
        }
    }
}