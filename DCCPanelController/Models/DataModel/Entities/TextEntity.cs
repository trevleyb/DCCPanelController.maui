using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : Entity, ITextEntity, IDrawingEntity, IRotationEntity {
    public override string Name => "Text Block";
    
    [ObservableProperty] [property: EditableString("Label", "", 0, "Text")]
    private string _label = string.Empty;
    
    [ObservableProperty] [property: EditableColor("Text Color", "", 0, group: "Text")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: EditableAlignment("Horizontal", "", 0, group: "Text")]
    private TextAlignment _horizontalJustification = TextAlignment.Center;
    
    [ObservableProperty] [property: EditableAlignment("Vertical", "", 0, group: "Text")]
    private TextAlignment _verticalJustification = TextAlignment.Center;

    [ObservableProperty] [property: EditableInt("Font Size", "", 0, group: "Text")]
    private int _fontSize = 8;

    [ObservableProperty] [property: EditableColor("Border Color", "", 5, group: "Border")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: EditableColor("Background Color", "", 5, group: "Border")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableInt("Border Radius", "", 5, group: "Border")] 
    private int _borderRadius = 0;
    
    [ObservableProperty] [property: EditableInt("Border Width", "", 5, group: "Border")]
    private int _borderWidth  = 0;
    
    [ObservableProperty] [property: EditableOpacity("Opacity", "", 5, group: "Border" )]
    private double _opacity = 0.5;
   
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