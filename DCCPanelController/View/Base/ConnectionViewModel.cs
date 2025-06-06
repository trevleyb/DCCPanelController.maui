using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View.Base;

public partial class ConnectionViewModel : BaseViewModel {
    protected ConnectionViewModel(Profile profile, ConnectionService connectionService) {
        Profile = profile;
        ConnectionService = connectionService;
        ConnectionService.ConnectionChanged += (sender, args) => {
            Console.WriteLine($"ConnectionViewModel: Connection State Changed to {args.Status} and IsConnected={args.IsConnected}");
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectionIcon));
        };
    }

    public Profile Profile { get; set; }
    public ConnectionService ConnectionService { get; }

    public bool IsConnected => ConnectionService.IsConnected;
    public string ConnectionIcon => ConnectionService.ConnectionIcon;

    [RelayCommand]
    protected async Task ToggleConnectionAsync() {
        var result = await ConnectionService.ToggleConnectionAsync();
        if (result.IsFailure) {
            var message = $"Unable to connect to the server{(string.IsNullOrEmpty(result.Message) ? "." : $" due to {result.Message}")}";            
            await DisplayAlertHelper.DisplayOkAlertAsync("Error Connecting", message);
        }
    }
}