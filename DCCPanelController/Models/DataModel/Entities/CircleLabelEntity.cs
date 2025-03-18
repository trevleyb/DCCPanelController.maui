using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : Entity, ITextEntity {
    public override string Name => "Circle Image";

    [ObservableProperty] [property: Editable("Label", EditableType.String, 0, group: "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: Editable("Text Color", EditableType.Color, 0, group: "Text")]
    private Color _textColor = Colors.White;

    [ObservableProperty] [property: Editable("Font Size", EditableType.Integer, 0, group: "Text")]
    private int _fontSize = 8;
    
    [ObservableProperty] [property: Editable("Border Radius", EditableType.Integer, 5, group: "Circle")]
    private int _borderRadius = 0;

    [ObservableProperty] [property: Editable("Border Width", EditableType.Integer, 5, group: "Circle")]
    private int _borderWidth = 0;
    
    [ObservableProperty] [property: Editable("Border Color", EditableType.Color, 5, group: "Circle")]
    private Color _borderColor = Colors.Transparent;
    
    [ObservableProperty] [property: Editable("Background Color", EditableType.Color, 5, group: "Circle")]
    private Color _backgroundColor = Colors.Transparent;
    
    [ObservableProperty][property: Editable("Opacity", EditableType.Opacity, 5, group: "Circle")] 
    private double _opacity = 1;

    [JsonConstructor]
    public CircleLabelEntity() { }

    public CircleLabelEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CircleLabelEntity(CircleLabelEntity entity) : base(entity) {
        BorderRadius = entity.BorderRadius;
        BorderWidth = entity.BorderWidth;
        BorderColor = entity.BorderColor;
        BackgroundColor = entity.BackgroundColor;
        FontSize = entity.FontSize;
        Label = entity.Label;
        TextColor = entity.TextColor;
        Opacity = entity.Opacity;
    }

    public override Entity Clone() => new CircleLabelEntity(this);
}