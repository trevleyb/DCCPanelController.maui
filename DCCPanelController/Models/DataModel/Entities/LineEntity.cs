using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
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

    public override string EntityName => "Line";
    public override string EntityDescription => "Straight Line";
    public override string EntityInformation => "";

    public override Entity Clone() => new LineEntity(this);
}