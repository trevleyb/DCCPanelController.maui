using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class TestPanelPageModel : BaseViewModel {
    [ObservableProperty] Panel? _panel;
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private bool _designMode = false;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    [ObservableProperty] private int _zoom = 10;

    public TestPanelPageModel() {
        IsRefreshing = true;
        IsBusy = true;
        
        var panels = new Panels();
        var panel = panels.CreatePanel();
        panels.Add(panel);
        panel.Rows = 18;
        panel.Cols = 27;
        
        var buttonEntity = panel.AddEntity(panel.CreateEntity<ButtonEntity>());
        var cornerEntity = panel.AddEntity(panel.CreateEntity<CornerEntity>());
        var cornerContinuationEntity = panel.AddEntity(panel.CreateEntity<CornerContinuationEntity>());
        var crossingEntity = panel.AddEntity(panel.CreateEntity<CrossingEntity>());
        var straightContinuationEntity = panel.AddEntity(panel.CreateEntity<StraightContinuationEntity>());
        var straightEntity = panel.AddEntity(panel.CreateEntity<StraightEntity>());
        straightEntity.Width = 2;
        straightEntity.Height = 2;
        var terminatorEntity = panel.AddEntity(panel.CreateEntity<TerminatorEntity>());
        var compassEntity = panel.AddEntity(panel.CreateEntity<CompassEntity>());

        var circleEntity = panel.AddEntity(panel.CreateEntity<CircleEntity>());
        var circleLabelEntity = panel.AddEntity(panel.CreateEntity<CircleLabelEntity>());
        var imageEntity = panel.AddEntity(panel.CreateEntity<ImageEntity>());
        var leftTurnoutEntity = panel.AddEntity(panel.CreateEntity<LeftTurnoutEntity>());
        var lineEntity = panel.AddEntity(panel.CreateEntity<LineEntity>());
        var pointsEntity = panel.AddEntity(panel.CreateEntity<PointsEntity>());
        var rectangleEntity = panel.AddEntity(panel.CreateEntity<RectangleEntity>());
        var rightTurnoutEntity = panel.AddEntity(panel.CreateEntity<RightTurnoutEntity>());

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
        
        IsRefreshing = false;
        IsBusy = false;

    }
}