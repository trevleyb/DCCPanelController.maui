using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class OperateViewModel : Base.ConnectionViewModel {
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showPath;

    public OperateViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        Panels = Profile.Panels.Where(p => p.Entities.Count > 0).ToObservableCollection();
        if (Panels.Any()) {
            SelectedPanel = Panels.FirstOrDefault();
        }
    }

    public Color BackgroundColor => SelectedPanel?.BackgroundColor ?? Colors.White;
    public ObservableCollection<Panel> Panels { get; set; }
}