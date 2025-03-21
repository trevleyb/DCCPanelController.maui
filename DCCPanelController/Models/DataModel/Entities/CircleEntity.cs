using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleEntity : Entity, IDrawingEntity {
    public override string Name => "Circle";

    [ObservableProperty] [property: EditableInt("Border Width", group: "Circle")]
    private int _borderWidth = 1;

    [ObservableProperty] [property: EditableColor("Background", group: "Circle")] 
    private Color _backgroundColor = Colors.Gray;
    
    [ObservableProperty] [property: EditableColor("Border", group: "Circle")]
    private Color _borderColor = Colors.Black;
    
    [ObservableProperty] [property: EditableOpacity("Opacity", group: "Circle" )]
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