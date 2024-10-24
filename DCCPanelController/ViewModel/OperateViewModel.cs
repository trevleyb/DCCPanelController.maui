using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.Tracks;

namespace DCCPanelController.ViewModel;

public partial class OperateViewModel : BaseViewModel {

    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private Panel? _selectedPanel;
    private readonly SettingsService _settingsService;
    public ObservableCollection<Panel> Panels  { get; set; }

    public OperateViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        PropertyChanged += OnPropertyChanged;

        Panels = _settingsService.Panels;
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        } 
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(ShowGrid):
            //if (SelectedPanel is not null) SelectedPanel.ShowGrid = ShowGrid;
            break;
        }
    }
}