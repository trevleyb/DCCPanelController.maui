using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View;

public partial class AboutViewModel : BaseViewModel {
    [ObservableProperty] Panel? _panel;
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private bool _designMode = false;
    [ObservableProperty] private int _zoom = 10;

    public AboutViewModel() {

        var panels = new Panels();
        var panel = panels.CreatePanel();
        panel.Rows = 10;
        panel.Cols = 15;

        for (var i = 0; i < 10; i++) {
            var entity = panel.CreateEntity<ButtonEntity>();
            entity.Col = i;
            entity.Row = i;
            entity.Width = 1;
            entity.Height = 1;
        }

        Panel = panel;
    }
}