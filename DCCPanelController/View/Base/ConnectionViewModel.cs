using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.View.Base;

public partial class ConnectionViewModel : BaseViewModel {
    private readonly             ProfileService                         _profileService;
    [ObservableProperty] private DccClientStatus                        _connectionState;
    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];

    protected ConnectionViewModel(ProfileService profileService, ConnectionService connectionService) {
        _profileService = profileService;
        ConnectionService = connectionService;
        ConnectionService.ConnectionStateChanged += (sender, args) => {
            ConnectionState = args;
            OnPropertyChanged(nameof(ConnectionIcon));
            OnPropertyChanged(nameof(IsConnectionAvailable));
        };
        ServerMessages = ConnectionService.ServerMessages;
    }

    private Profile Profile => _profileService?.ActiveProfile ?? throw new ArgumentNullException(nameof(Profile), "ConnectionViewModel: Active profile is not defined.");
    public ConnectionService ConnectionService { get; }
    public bool IsConnectionAvailable => Profile?.Settings?.ClientSettings?.HasValidSettings ?? false;
    public bool IsConnected => ConnectionState == DccClientStatus.Connected;
    
    public string ConnectionIcon =>
        ConnectionState switch {
            DccClientStatus.Connected    => "wifi_on",
            DccClientStatus.Disconnected => "wifi_off",
            DccClientStatus.Error        => "wifi_error",
            DccClientStatus.Reconnecting => "wifi_reconnecting",
            DccClientStatus.Initialising => "wifi_initialising",
            _                            => "wifi_off",
        };
        
    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        var result = await ConnectionService.ToggleConnectionAsync();
        if (result.IsFailure) {
            var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        }
    }
}