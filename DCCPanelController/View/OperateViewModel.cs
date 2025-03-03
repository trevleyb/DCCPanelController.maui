using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class OperateViewModel : BaseViewModel {
    [ObservableProperty] private bool? _isConnected;
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showPath;

    public OperateViewModel() {
        var settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        Panels = settingsService.Panels;

        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
    }

    public Color BackgroundColor => SelectedPanel?.BackgroundColor ?? Colors.White;
    public ObservableCollection<Panel> Panels { get; set; }

    public string SetActivePanel(Panel? panelCarouselCurrentItem) {
        SelectedPanel = panelCarouselCurrentItem;
        OnPropertyChanged(nameof(BackgroundColor));
        return SelectedPanel?.Name ?? "Control Panel";
    }

    // TODO: Add support for Connecting/Disconnecting
    // TODO: Add events to change the Icon on Connection Status
    public void Connect() {
        IsConnected = IsConnected switch {
            true  => false,
            false => null,
            null  => true
        };
    }
}