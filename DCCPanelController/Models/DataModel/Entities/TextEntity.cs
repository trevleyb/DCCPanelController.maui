using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : Entity, ITextEntity, IDrawingEntity {
    public override string Name => "Text Block";
    
    [ObservableProperty] [property: Editable("Label", EditableType.String, 0, "Text")]
    private string _label = string.Empty;
    
    [ObservableProperty] [property: Editable("Text Color", EditableType.Color, 0, group: "Text")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: Editable("Horizontal", EditableType.Alignment, 0, group: "Text")]
    private TextAlignment _horizontalJustification = TextAlignment.Center;
    
    [ObservableProperty] [property: Editable("Vertical", EditableType.Alignment, 0, group: "Text")]
    private TextAlignment _verticalJustification = TextAlignment.Center;

    [ObservableProperty] [property: Editable("Font Size", EditableType.Integer, 0, group: "Text")]
    private int _fontSize = 8;

    [ObservableProperty] [property: Editable("Border Color", EditableType.Color, 5, group: "Border")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: Editable("Background Color", EditableType.Color, 5, group: "Border")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: Editable("Border Radius", EditableType.Integer, 5, group: "Border")] 
    private int _borderRadius = 0;
    
    [ObservableProperty] [property: Editable("Border Width", EditableType.Integer, 5, group: "Border")]
    private int _borderWidth  = 0;
    
   
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