using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : Entity, ITextEntity {
    public override string Name => "Text Block";
    
    [ObservableProperty] [property: Editable("Border Radius", EditableType.Integer, group: "Border")] 
    private int _borderRadius = 0;
    
    [ObservableProperty] [property: Editable("Border Width", EditableType.Integer, group: "Border")]
    private int _borderWidth  = 0;
    
    [ObservableProperty] [property: Editable("Font Size", EditableType.Integer, group: "Text")]
    private int _fontSize = 8;
    
    [ObservableProperty] [property: Editable("Label", EditableType.Integer)]
    private string _label = string.Empty;
    
    [ObservableProperty] [property: Editable("Text Color", EditableType.Color, group: "Text")]
    private Color _textColor = Colors.White;
    
    [ObservableProperty] [property: Editable("Border Color", EditableType.Color, group: "Border")]
    private Color _borderColor = Colors.Transparent;
    
    [ObservableProperty] [property: Editable("background Color", EditableType.Color, group: "Border")]
    private Color _backgroundColor = Colors.Transparent;
    
    [ObservableProperty] [property: Editable("Horizontal Alignment", EditableType.Alignment, group: "Text")]
    private TextAlignment _horizontalJustification = TextAlignment.Center;
    
    [ObservableProperty] [property: Editable("Vertical Alignment", EditableType.Alignment, group: "Text")]
    private TextAlignment _verticalJustification = TextAlignment.Center;
    
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
        Label = entity.Label;
        HorizontalJustification = entity.HorizontalJustification;
        VerticalJustification = entity.VerticalJustification;
    }
    public override Entity Clone() => new TextEntity(this);
}