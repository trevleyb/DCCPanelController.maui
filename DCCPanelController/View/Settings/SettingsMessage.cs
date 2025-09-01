using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.View.Settings;

public partial class SettingsMessage(string message, bool clear = false) : ObservableObject {
    [ObservableProperty] private bool _clear = clear;
    [ObservableProperty] private string _message = message;
    [ObservableProperty] private DateTime _timeStamp = DateTime.Now;
}