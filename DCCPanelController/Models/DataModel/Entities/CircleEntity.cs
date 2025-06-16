using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleEntity : Entity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Background", group: "Circle")]
    private Color _backgroundColor = Colors.Gray;

    [ObservableProperty] [property: EditableColor("Border", group: "Circle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableInt("Border Width", group: "Circle")]
    private int _borderWidth = 1;
    
    [JsonConstructor]
    public CircleEntity() { }

    public CircleEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CircleEntity(CircleEntity entity) : base(entity) {
        BackgroundColor = entity.BackgroundColor;
        BorderColor = entity.BorderColor;
        BorderWidth = entity.BorderWidth;
        Opacity = entity.Opacity;
    }

    public override string EntityName => "Circle";

    public override Entity Clone() {
        return new CircleEntity(this);
    }
}