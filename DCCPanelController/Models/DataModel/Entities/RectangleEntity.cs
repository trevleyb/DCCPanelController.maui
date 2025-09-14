using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RectangleEntity : DrawingEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Background Color", Group = "Rectangle")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: Editable("Border Color", Group = "Rectangle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: Editable("Border Radius", Group = "Rectangle")]
    private int _borderRadius;

    [ObservableProperty] [property: Editable("Border Width", Group = "Rectangle")]
    private int _borderWidth = 1;

    [JsonConstructor]
    public RectangleEntity() { }

    public RectangleEntity(Panel panel) : this() => Parent = panel;

    public RectangleEntity(RectangleEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public override string EntityName => "Rectangle";
    public override string EntityDescription => "Adjustable Box";
    public override string EntityInformation =>
        "This is a **drawable** rectangle which allows you to draw squares and rectangles with a border and internal color. You can set the *opacity* and show it underneath other tracks and objects.";

    public override Entity Clone() => new RectangleEntity(this);
}