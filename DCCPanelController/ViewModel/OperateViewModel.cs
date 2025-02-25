using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class OperateViewModel : BaseViewModel {
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;

    public OperateViewModel() {
        var settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        Panels = settingsService.Panels;
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
    }

    public Color BackgroundColor => SelectedPanel?.Defaults.BackgroundColor ?? Colors.White;

    public ObservableCollection<Panel> Panels { get; set; }

    public string SetActivePanel(Panel? panelCarouselCurrentItem) {
        SelectedPanel = panelCarouselCurrentItem;
        OnPropertyChanged(nameof(BackgroundColor));
        return SelectedPanel?.Name ?? "Control Panel";
    }

    private void SelectedPanelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        throw new NotImplementedException();
    }
}