using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class RectangleEntity : Entity, IDrawingEntity {
    public override string Name => "Rectangle";
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private Color _borderColor = Colors.Black;
    [ObservableProperty] private int _borderWidth  = 1;
    [ObservableProperty] private double _opacity = 1;
    
    [JsonConstructor]
    public RectangleEntity() {}
    public RectangleEntity(Panel panel) : this() {
        Parent = panel;
    }
    public RectangleEntity(RectangleEntity entity) : base(entity) {
        BackgroundColor = entity.BackgroundColor;
        BorderColor = entity.BorderColor;
        BorderWidth = entity.BorderWidth;
        Opacity = entity.Opacity;
    }
    public override Entity Clone() => new RectangleEntity(this);
}