using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.BaseClasses;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleEntity : DrawingEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Background", "", 0, "Circle")]
    private Color _backgroundColor = Colors.Gray;

    [ObservableProperty] [property: Editable("Border", "", 0, "Circle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty]
    [property: Editable("Border Width", "", 0, "Circle")]
    private int _borderWidth = 1;

    [JsonConstructor]
    public CircleEntity() { }

    public CircleEntity(Panel panel) : this() => Parent = panel;

    public CircleEntity(CircleEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;

    [JsonIgnore] public override string EntityName => "Circle";
    [JsonIgnore] public override string EntityDescription => "Adjustable Circle";
    [JsonIgnore] public override string EntityInformation =>
        "This is a **drawable** circle which allows you to draw ovals and circles with a border and internal color. You can set the *opacity* and show it underneath other tracks and objects.";

    public override Entity Clone() => new CircleEntity(this);
}