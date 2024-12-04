using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class OperateViewModel : BaseViewModel {
    private readonly SettingsService _settingsService;
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;

    public OperateViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        PropertyChanged += OnPropertyChanged;

        Panels = _settingsService.Panels;
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
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