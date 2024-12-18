using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class OperateViewModel : BaseViewModel {
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;

    public Color BackgroundColor => SelectedPanel?.Defaults.BackgroundColor ?? Colors.White; 
    
    public OperateViewModel() {
        var settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        Panels = settingsService.Panels;
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
        PropertyChanged += OnPropertyChanged;
    }
    
    public string SetActivePanel(Panel? panelCarouselCurrentItem) {
        SelectedPanel = panelCarouselCurrentItem;
        OnPropertyChanged(nameof(BackgroundColor));
        return SelectedPanel?.Name ?? "Control Panel";
    }

    private void SelectedPanelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        throw new NotImplementedException();
    }

    public ObservableCollection<Panel> Panels { get; set; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(ShowGrid):
            //if (SelectedPanel is not null) SelectedPanel.ShowGrid = ShowGrid;
            break;
        }
    }
}