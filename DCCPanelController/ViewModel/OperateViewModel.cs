using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class OperateViewModel : BaseViewModel {
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private Color _backgroundColor = Colors.White;
    
    public OperateViewModel() {
        var settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        Panels = settingsService.Panels;
        BackgroundColor = settingsService.Settings.BackgroundColor;
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
        PropertyChanged += OnPropertyChanged;
    }
    
    public string SetActivePanel(Panel? panelCarouselCurrentItem) {
        SelectedPanel = panelCarouselCurrentItem;
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