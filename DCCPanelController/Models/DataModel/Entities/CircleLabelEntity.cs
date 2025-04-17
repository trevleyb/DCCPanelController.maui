using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : Entity, ITextEntity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Background Color", "", 5, "Circle")]
    private Color _backgroundColor = Colors.DarkGray;

    [ObservableProperty] [property: EditableColor("Border Color", "", 5, "Circle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableColor("Inner Border Color", "", 5, "Circle")]
    private Color _borderInnerColor = Colors.White;

    [ObservableProperty] [property: EditableInt("Inner Border Gap", "", 5, "Circle")]
    private int _borderInnerGap = 2;

    [ObservableProperty] [property: EditableInt("Inner Border Width", "", 5, "Circle")]
    private int _borderInnerWidth = 2;

    [ObservableProperty] [property: EditableInt("Border Width", "", 5, "Circle")]
    private int _borderWidth = 2;

    [ObservableProperty] [property: EditableInt("Font Size", "", 0, "Text")]
    private int _fontSize = 8;

    [ObservableProperty] [property: EditableString("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableOpacity("Opacity", "", 5, "Circle")]
    private double _opacity = 1;

    [ObservableProperty] [property: EditableDouble("Scale", "", 5, "Circle", .25, 2.0)]
    private double _scale = 1;

    [ObservableProperty] [property: EditableColor("Text Color", "", 0, "Text")]
    private Color _textColor = Colors.White;

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

    public override string EntityName => "Circle Label";

    public override Entity Clone() {
        return new CircleLabelEntity(this);
    }
}