using System.Reflection;

namespace DCCPanelController.View.Helpers;

public static class VersionInfo {
    public static string Version {
        get {
            var infoVersion = typeof(VersionInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            return!string.IsNullOrWhiteSpace(infoVersion) ? $"Version {infoVersion}" : $"Version {AppInfo.Current.VersionString}";
        }
    }
}