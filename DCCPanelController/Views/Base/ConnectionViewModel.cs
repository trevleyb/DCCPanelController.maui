using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.Views.Base;

public partial class ConnectionViewModel : BaseViewModel {
    private ProfileService _profileService;
    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];
    [ObservableProperty] private bool _isConnected = false;
    
    protected ConnectionViewModel(ProfileService profileService, ConnectionService connectionService) {
        _profileService = profileService;
        ConnectionService = connectionService;
        ConnectionService.ConnectStateChanged += (sender, args) => {
            IsConnected = args;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectionIcon));
            OnPropertyChanged(nameof(IsConnectionAvailable));
        };
        ServerMessages = ConnectionService.ServerMessages;
    }

    private Profile Profile => _profileService?.ActiveProfile ?? throw new ArgumentNullException(nameof(Profile),"ConnectionViewModel: Active profile is not defined.");
    public ConnectionService ConnectionService { get; }
    public bool IsConnectionAvailable => Profile?.Settings?.ClientSettings?.HasValidSettings ?? false;
    
    public string IconWifiOn => "wifi_on";
    public string IconWifiOff => "wifi_off";
    public string ConnectionIcon => IsConnected ? IconWifiOn : IconWifiOff;
    
    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        var result = await ConnectionService.ToggleConnectionAsync();
        if (result.IsFailure) {
            var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";            
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        }
    }
}