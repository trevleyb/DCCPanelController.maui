using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : Entity, ITextEntity {
    public override string Name => "Circle Label";

    [ObservableProperty] [property: EditableString("Label", "", 0, group: "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableColor("Text Color", "", 0, group: "Text")]
    private Color _textColor = Colors.White;

    [ObservableProperty] [property: EditableInt("Font Size", "", 0, group: "Text")]
    private int _fontSize = 8;
    
    [ObservableProperty] [property: EditableDouble("Scale Factor", "", 0, group: "Text")]
    private double _scaleFactor = 1.00;

    [ObservableProperty] [property: EditableInt("Border Width", "", 5, group: "Circle")]
    private int _borderWidth = 2;
    
    [ObservableProperty] [property: EditableColor("Border Color", "", 5, group: "Circle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableInt("Inner Border Width", "", 5, group: "Circle")]
    private int _borderInnerWidth = 2;

    [ObservableProperty] [property: EditableColor("Inner Border Color", "", 5, group: "Circle")]
    private Color _borderInnerColor = Colors.White;

    [ObservableProperty] [property: EditableInt("Inner Border Gap", "", 5, group: "Circle")]
    private int _borderInnerGap = 2;
    
    [ObservableProperty] [property: EditableColor("Background Color", "", 5, group: "Circle")]
    private Color _backgroundColor = Colors.DarkGray;
    
    [ObservableProperty][property: EditableOpacity("Opacity", "", 5, group: "Circle")] 
    private double _opacity = 1;

    [JsonConstructor]
    public CircleLabelEntity() { }

    public CircleLabelEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CircleLabelEntity(CircleLabelEntity entity) : base(entity) {
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