using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Base;
using SceneKit;

namespace DCCPanelController.View;

public partial class PanelSurfaceEditorViewModel : BaseViewModel {
 
    [ObservableProperty] 
    private Panel? _selectedPanel;
    
    public PanelSurfaceEditorViewModel() {
        
        var panels = new Panels();
        var panel = panels.CreatePanel();
        panel.Id = "Test";
        panel.Rows = 10;
        panel.Cols = 18;
        panel.AddEntity(new StraightEntity() { Col = 1, Row = 1, Rotation = 45 } );
        panel.AddEntity(new StraightEntity() { Col = 2, Row = 2, Rotation = 90 } );
        panel.AddEntity(new StraightEntity() { Col = 3, Row = 3, Rotation = 135 } );
        panel.AddEntity(new StraightEntity() { Col = 4, Row = 4, Rotation = 180 } );
        panel.AddEntity(new StraightEntity() { Col = 5, Row = 5, Rotation = 225 } );
        SelectedPanel = panel;
    }
}