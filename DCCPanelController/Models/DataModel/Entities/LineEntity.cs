using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class LineEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Line Color", group: "Line")]
    private Color _lineColor = Colors.Black;

    [ObservableProperty] [property: EditableInt("Line Width", group: "Line")]
    private int _lineWidth = 3;

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

    public override string EntityName => "Line";

    public override Entity Clone() {
        return new LineEntity(this);
    }
}