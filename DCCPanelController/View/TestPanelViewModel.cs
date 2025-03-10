using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View;

public partial class TestPanelPageModel : BaseViewModel {
    [ObservableProperty] Panel? _panel;
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private bool _designMode = false;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    [ObservableProperty] private int _zoom = 10;

    public TestPanelPageModel() {

        var panels = new Panels();
        var panel = panels.CreatePanel();
        panel.Rows = 10;
        panel.Cols = 15;

        for (var i = 0; i < 10; i++) {
            Entity? entity = null;
            if (i % 2 == 0) entity = panel.CreateEntity<ButtonEntity>();
            if (i % 2 == 1) entity = panel.CreateEntity<StraightEntity>();
            if (entity is not null) {
                entity.Col = i;
                entity.Row = i;
                entity.Width = 1;
                entity.Height = 1;
            }
        }

        Panel = panel;
    }
}