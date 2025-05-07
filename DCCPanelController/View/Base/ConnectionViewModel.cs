using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class ConnectionViewModel : BaseViewModel {

    protected Profile Profile { get; }
    protected ConnectionService ConnectionService { get; }

    public bool IsConnected => ConnectionService.IsConnected;
    public string ConnectionIcon => ConnectionService.ConnectionIcon;
    
    protected ConnectionViewModel(Profile profile, ConnectionService connectionService) {
        Profile = profile;
        ConnectionService = connectionService;
        ConnectionService.ConnectionChanged += (sender, args) => {
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectionIcon));
        }; 
    }
    
    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        await ConnectionService.ToggleConnectionAsync();
    }
    
    protected async Task<bool> AskUserToConfirm(string title, string message) {
        if (App.Current.Windows[0].Page is { } window) {
            var result = await window.DisplayAlert(
                title,
                message,
                "Yes",
                "No"
            );
            return result;
        }
        return false;
    }
}