using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DccClients.Jmri.Client;
using DCCCommon.Client;

namespace DCCPanelController.Models.DataModel;

public partial class Settings : ObservableObject {
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private bool _connectOnStartup = true;
    [ObservableProperty] private IDccClientSettings? _clientSettings = new JmriClientSettings();
}