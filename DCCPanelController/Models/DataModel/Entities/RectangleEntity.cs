using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RectangleEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Background Color", group: "Rectangle")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableColor("Border Color", group: "Rectangle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableInt("Border Radius", group: "Rectangle")]
    private int _borderRadius;

    [ObservableProperty] [property: EditableInt("Border Width", group: "Rectangle")]
    private int _borderWidth = 1;

    [JsonConstructor]
    public RectangleEntity() { }

    public RectangleEntity(Panel panel) : this() {
        Parent = panel;
    }

    public RectangleEntity(RectangleEntity entity) : base(entity) {
        BackgroundColor = entity.BackgroundColor;
        BorderColor = entity.BorderColor;
        BorderWidth = entity.BorderWidth;
        Opacity = entity.Opacity;
    }

    public override string EntityName => "Rectangle";

    public override Entity Clone() {
        return new RectangleEntity(this);
    }
}