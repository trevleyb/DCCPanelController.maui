using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : Entity {
    public override string Name => "Text Block";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth  = 0;
    [ObservableProperty] private int _fontSize = 8;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private Color _textColor = Colors.White;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private FontWeight _fontWeight = FontWeight.Regular;
    [ObservableProperty] private TextAlignment _horizontalJustification = TextAlignment.Center;
    [ObservableProperty] private TextAlignment _verticalJustification = TextAlignment.Center;
    
    [JsonConstructor]
    public TextEntity() {}
    public TextEntity(Panel panel) : this() {
        Parent = panel;
    }
    public TextEntity(TextEntity entity) : base(entity) {
        BorderRadius = entity.BorderRadius;
        BorderWidth = entity.BorderWidth;
        TextColor = entity.TextColor;
        BorderColor = entity.BorderColor;
        BackgroundColor = entity.BackgroundColor;
        FontSize = entity.FontSize;
        FontWeight = entity.FontWeight;
        Label = entity.Label;
        HorizontalJustification = entity.HorizontalJustification;
        VerticalJustification = entity.VerticalJustification;
    }
    public override Entity Clone() => new TextEntity(this);
}