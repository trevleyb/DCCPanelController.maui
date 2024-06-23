using DCCPanelController.Model;

namespace DCCPanelController.Services;

public class SettingsService { 
    public Settings GetSettings() {
        var settings = App.ServiceProvider?.GetService<StorageService>();
        if (settings == null) throw new Exception("StorageService not found");
        return settings.Settings;
    }
}