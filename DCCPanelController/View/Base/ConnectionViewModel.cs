using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.View.Base;

public partial class ConnectionViewModel : BaseViewModel {
    private readonly ProfileService _profileService;

    public ConnectionService ConnectionService { get; }
    public DccClientState ConnectionState => ConnectionService?.ConnectionState ?? DccClientState.Disconnected;
    private Profile Profile => _profileService?.ActiveProfile ?? throw new ArgumentNullException(nameof(Profile), "ConnectionViewModel: Active profile is not defined.");

    public bool IsConnectionAvailable => Profile?.Settings?.ClientSettings?.HasValidSettings ?? false;
    public bool IsConnected => ConnectionState == DccClientState.Connected;

    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];

    protected ConnectionViewModel(ProfileService profileService, ConnectionService connectionService) {
        _profileService = profileService;
        ConnectionService = connectionService;
        ConnectionService.ConnectionStateChanged += (sender, args) => {
            OnPropertyChanged(nameof(ConnectionState));
            OnPropertyChanged(nameof(ConnectionIcon));
            OnPropertyChanged(nameof(ConnectionText));
            OnPropertyChanged(nameof(ConnectionColor));
            OnPropertyChanged(nameof(IsConnectionAvailable));
        };
        ServerMessages = ConnectionService.ServerMessages;
    }

    public string ConnectionText =>
        ConnectionState switch {
            DccClientState.Connected    => "Connected",
            DccClientState.Disconnected => "Disconnected",
            DccClientState.Error        => "Error",
            DccClientState.Initialising => "Initialising",
            DccClientState.Reconnecting => "Reconnecting",
            _                           => "Unknown Connection State"
        };

    public Color ConnectionColor =>
        ConnectionState switch {
            DccClientState.Connected    => Colors.LightGreen,
            DccClientState.Disconnected => Colors.Gray,
            DccClientState.Error        => Colors.Red,
            DccClientState.Initialising => Colors.Blue,
            DccClientState.Reconnecting => Colors.Yellow,
            _                           => Colors.Red
        };

    public string ConnectionIcon =>
        ConnectionState switch {
            DccClientState.Connected    => "wifi_on",
            DccClientState.Disconnected => "wifi_off",
            DccClientState.Error        => "wifi_error",
            DccClientState.Reconnecting => "wifi_search",
            DccClientState.Initialising => "wifi_search",
            _                           => "wifi_off",
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