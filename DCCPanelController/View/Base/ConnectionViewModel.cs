using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View.Base;

public partial class ConnectionViewModel : BaseViewModel {

    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];
    [ObservableProperty] private bool _isConnected = false;
    
    protected ConnectionViewModel(Profile profile, ConnectionService connectionService) {
        Profile = profile;
        ConnectionService = connectionService;
        ConnectionService.ConnectStateChanged += (sender, args) => {
            IsConnected = args;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectionIcon));
            OnPropertyChanged(nameof(IsConnectionAvailable));
        };
        ServerMessages = ConnectionService.ServerMessages;
    }

    public Profile Profile { get; set; }
    public ConnectionService ConnectionService { get; }
    public bool IsConnectionAvailable => true; 
    public string ConnectionIcon => IsConnected ? "server_on.png" : "server_off.png";

    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        var result = await ConnectionService.ToggleConnectionAsync();
        if (result.IsFailure) {
            var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";            
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        }
    }
}