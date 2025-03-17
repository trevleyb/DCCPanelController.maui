using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleEntity : Entity, IDrawingEntity {
    public override string Name => "Circle";

    [ObservableProperty] [property: Editable("Border Width", EditableType.Integer, group: "Circle")]
    private int _borderWidth = 1;

    [ObservableProperty] [property: Editable("Background", EditableType.Color, group: "Circle")] 
    private Color _backgroundColor = Colors.Gray;
    
    [ObservableProperty] [property: Editable("Border", EditableType.Color, group: "Circle")]
    private Color _borderColor = Colors.Black;
    
    [ObservableProperty] [property: Editable("Opacity", EditableType.Double, group: "Circle" )]
    private double _opacity = 0.5;
    
    [JsonConstructor]
    public CircleEntity() {}
    public CircleEntity(Panel panel) : this() {
        Parent = panel;
    }
    
    public CircleEntity(CircleEntity entity) : base(entity) {
        BackgroundColor = entity.BackgroundColor;
        BorderColor = entity.BorderColor;
        BorderWidth = entity.BorderWidth;
        Opacity = entity.Opacity;
    }
    public override Entity Clone() => new CircleEntity(this);
}