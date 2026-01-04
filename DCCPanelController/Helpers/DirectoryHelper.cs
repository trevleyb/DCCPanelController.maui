namespace DCCPanelController.Helpers;

public static class DirectoryHelper {

    public static string GetProfileDirectory()  => GetDirectory("Profiles");
    public static string GetLogDirectory()      => GetDirectory("Logs");
    public static string GetTempDirectory()     => GetDirectory("Temp");
    
    public static string GetProfile(string filename) => Path.Combine(GetProfileDirectory(), filename);
    public static string GetLogFile() => Path.Combine(GetLogDirectory(), "DccPanelController.log");
    public static string GetTempFile() => Path.Combine(GetTempDirectory(), Guid.NewGuid().ToString() + ".tmp");
    
    public static string GetDirectory(string subDir) {
        var directory = "";

        #if IOS || MACCATALYST
        directory = string.IsNullOrEmpty(subDir) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), subDir);
        #elif ANDROID
        directory = string.IsNullOrEmpty(subDir) ? Path.Combine(FileSystem.Current.AppDataDirectory, "Logs") : FileSystem.Current.AppDataDirectory;
        #elif WINDOWS
        directory = string.IsNullOrEmpty(subDir) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DCCPanelController", "Logs") :Environment.SpecialFolder.LocalApplicationData);
        #else
        directory = string.IsNullOrEmpty(subDir) ? Path.Combine(FileSystem.Current.AppDataDirectory, "Logs") : FileSystem.Current.AppDataDirectory;
        #endif
        Directory.CreateDirectory(directory);
        return directory;
    }
}