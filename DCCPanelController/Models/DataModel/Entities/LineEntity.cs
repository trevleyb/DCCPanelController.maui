using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.BaseClasses;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class LineEntity : DrawingEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Line Color", "", 0, "Line")]
    private Color _lineColor = Colors.Black;

    [ObservableProperty] [property: Editable("Line Width", "", 0, "Line")]
    private int _lineWidth = 3;

    [JsonConstructor]
    public LineEntity() { }

    public LineEntity(Panel panel) : this() => Parent = panel;

    public LineEntity(LineEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;

    [JsonIgnore] public override string EntityName => "Line";
    [JsonIgnore] public override string EntityDescription => "Straight Line";
    [JsonIgnore] public override string EntityInformation =>
        "The Line tile is a straight line. The line can be used to draw a line between 2 points and is either centered left-to-right or top-to-bottom or if you rotate it then it will be corner-to-corner. You can set the color and opacity of the line.";

    public override Entity Clone() => new LineEntity(this);
}