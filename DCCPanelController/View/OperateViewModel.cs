using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class OperateViewModel : BaseViewModel {
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showPath;

    public OperateViewModel(Profile profile, ConnectionService connectionService) {
        ConnectionService = connectionService;
        ConnectionService.ConnectionChanged += (sender, args) => {
            Console.WriteLine($"Connection Service Event Raised: {args.IsConnected}");
            IsConnected = args.IsConnected;
        };
        Panels = profile.Panels;
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
    }

    private ConnectionService ConnectionService { get; }

    public Color BackgroundColor => SelectedPanel?.BackgroundColor ?? Colors.White;
    public ObservableCollection<Panel> Panels { get; set; }

    public string SetActivePanel(Panel? panelCarouselCurrentItem) {
        SelectedPanel = panelCarouselCurrentItem;
        OnPropertyChanged(nameof(BackgroundColor));
        return SelectedPanel?.Id ?? "Control Panel";
    }

    [RelayCommand]
    private async Task ToggleConnectionAsync() {
        if (!IsConnected) {
            await ConnectionService.Connect();
        } else {
            ConnectionService.Disconnect();
        }
    }
}