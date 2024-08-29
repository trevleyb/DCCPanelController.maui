using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Tracks;

namespace DCCPanelController.ViewModel;

public partial class OperateViewModel : BaseViewModel {

    [ObservableProperty] private bool _showGrid;
    public ObservableCollection<Panel> Panels { get; } = [];
    public Panel? SelectedPanel { get; set; }

    public OperateViewModel() {
        Panels = Services.SampleData.Panels.DemoData();
        SelectedPanel = Panels[0];
        this.PropertyChanged += OnPropertyChanged;
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(ShowGrid):
            //if (SelectedPanel is not null) SelectedPanel.ShowGrid = ShowGrid;
            break;
        }
    }
}