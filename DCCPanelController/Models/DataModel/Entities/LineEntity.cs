using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class LineEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Line Color", "",0, "Line")]
    private Color _lineColor = Colors.Black;

    [ObservableProperty] [property: EditableInt("Line Width", "",0, "Line")]
    private int _lineWidth = 3;

    [JsonConstructor]
    public LineEntity() { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public LineEntity(Panel panel) : this() {
        Parent = panel;
    }

    public LineEntity(LineEntity entity) : base(entity) { }

    public override string EntityName => "Line";
    public override string EntityDescription => "Straight Line";
    
    public override Entity Clone() {
        return new LineEntity(this);
    }
}