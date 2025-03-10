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
        panel.Cols = 16;

        var buttonEntity = panel.CreateEntity<ButtonEntity>();
        var cornerButtonEntity = panel.CreateEntity<CornerButtonEntity>();
        var cornerEntity = panel.CreateEntity<CornerEntity>();
        var cornerContinuationEntity = panel.CreateEntity<CornerContinuationEntity>();
        var crossingEntity = panel.CreateEntity<CrossingEntity>();
        var straightContinuationEntity = panel.CreateEntity<StraightContinuationEntity>();
        var straightEntity = panel.CreateEntity<StraightEntity>();
        straightEntity.Width = 2;
        straightEntity.Height = 2;
        var terminatorEntity = panel.CreateEntity<TerminatorEntity>();
        var compassEntity = panel.CreateEntity<CompassEntity>();

        var circleEntity = panel.CreateEntity<CircleEntity>();
        var circleLabelEntity = panel.CreateEntity<CircleLabelEntity>();
        var imageEntity = panel.CreateEntity<ImageEntity>();
        var leftTurnoutEntity = panel.CreateEntity<LeftTurnoutEntity>();
        var lineEntity = panel.CreateEntity<LineEntity>();
        var pointsEntity = panel.CreateEntity<PointsEntity>();
        var rectangleEntity = panel.CreateEntity<RectangleEntity>();
        var rightTurnoutEntity = panel.CreateEntity<RightTurnoutEntity>();

        var col = 1;
        var row = 1;
        foreach (var entity in panel.Entities) {
            entity.Col = col;
            entity.Row = row;
            entity.Width = 1;
            entity.Height = 1;
            col+= 2;
            if (col > panel.Cols) {
                col = col - panel.Cols;
                row+= 2;
            }
        }
       
        Panel = panel;
    }
}