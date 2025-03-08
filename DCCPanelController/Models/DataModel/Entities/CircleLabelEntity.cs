using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : Entity {
    public override string Name => "Circle Image";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth  = 0;
    [ObservableProperty] private int _fontSize = 8;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private Color _textColor = Colors.White;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private FontWeight _fontWeight = FontWeight.Regular;
    [ObservableProperty] private double _opacity = 1;
    
    [JsonConstructor]
    public CircleLabelEntity() {}
    public CircleLabelEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CircleLabelEntity(CircleLabelEntity entity) : base( entity) {
        BorderRadius = entity.BorderRadius;
        BorderWidth = entity.BorderWidth;
        BorderColor = entity.BorderColor;
        BackgroundColor = entity.BackgroundColor;
        FontSize = entity.FontSize;
        FontWeight = entity.FontWeight;
        Label = entity.Label;
        TextColor = entity.TextColor;
        Opacity = entity.Opacity;
    }
    public override Entity Clone() => new CircleLabelEntity(this);
}