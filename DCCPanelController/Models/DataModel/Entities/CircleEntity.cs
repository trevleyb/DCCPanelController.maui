using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleEntity : Entity {
    public override string Name => "Circle";
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private Color _borderColor = Colors.Black;
    [ObservableProperty] private int _borderWidth = 1;
    [ObservableProperty] private double _opacity = 1;
    
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