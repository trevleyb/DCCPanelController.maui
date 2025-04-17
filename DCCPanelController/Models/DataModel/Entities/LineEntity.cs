using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.DynamicProperties;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class LineEntity : Entity, IDrawingEntity, IRotationEntity {

    public override string EntityName => "Line";
    
    [ObservableProperty][property: EditableColor("Line Color", group: "Line")] 
    private Color _lineColor = Colors.Black;
    
    [ObservableProperty] [property: EditableInt("Line Width", group: "Line")]
    private int _lineWidth = 3;
    
    [ObservableProperty] [property: EditableOpacity("Opacity", group: "Line")]
    private double _opacity = 1;

    [JsonConstructor]
    public LineEntity() { }
    public LineEntity(Panel panel) : this() {
        Parent = panel;
    }
    public LineEntity(LineEntity entity) : base(entity) {
        LineColor = entity.LineColor;
        LineWidth = entity.LineWidth;
        Opacity = entity.Opacity;
        RotationFactor = 10;
    }
    public override Entity Clone() => new LineEntity(this);
}