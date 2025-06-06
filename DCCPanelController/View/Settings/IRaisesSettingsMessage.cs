using System.ComponentModel;

namespace DCCPanelController.View.Settings;

public interface IRaisesSettingsMessage : INotifyPropertyChanged {
    event EventHandler<SettingsMessage>? OnSettingsMessage;
}