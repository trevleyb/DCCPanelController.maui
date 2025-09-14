using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : DrawingEntity, ITextEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Background Color", "", 5, "Circle")]
    private Color _backgroundColor = Colors.DarkGray;

    [ObservableProperty] [property: Editable("Border Color", "", 5, "Circle")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: Editable("Inner Border Color", "", 5, "Circle")]
    private Color _borderInnerColor = Colors.White;

    [ObservableProperty] [property: Editable("Inner Border Gap", "", 5, "Circle")]
    private int _borderInnerGap = 2;

    [ObservableProperty] [property: Editable("Inner Border Width", "", 5, "Circle")]
    private int _borderInnerWidth = 2;

    [ObservableProperty] [property: Editable("Border Width", "", 5, "Circle")]
    private int _borderWidth = 2;

    [ObservableProperty] [property: Editable("Font Size", "", 1, "Text")]
    private int _fontSize = 15;

    [ObservableProperty] [property: Editable("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty]
    private double _scale = 0.8;

    [ObservableProperty] [property: Editable("Text Color", "", 2, "Text")]
    private Color _textColor = Colors.White;

    [JsonConstructor]
    public CircleLabelEntity() { }

    public CircleLabelEntity(Panel panel) : this() {
        Parent = panel;
        Layer = 8;
    }

    public CircleLabelEntity(CircleLabelEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public override string EntityName => "Label";
    public override string EntityDescription => "Circle Label";
    public override string EntityInformation => "";

    public override Entity Clone() => new CircleLabelEntity(this);
}