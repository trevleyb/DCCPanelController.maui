namespace DCCPanelController.Helpers.Logging;

public static class FileLogger {
    private static readonly string LogPath = Path.Combine(FileSystem.AppDataDirectory, "testflight_log.txt");
    public static void Log(string message) {
        try {
            var logLine = $"{DateTime.Now:HH:mm:ss}: {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, logLine);
            Console.WriteLine(message); 
        } catch { /* ignore logging failures */ }
    }
    
    public static void Clear() {
        if (File.Exists(LogPath)) File.Delete(LogPath);
    }
}
