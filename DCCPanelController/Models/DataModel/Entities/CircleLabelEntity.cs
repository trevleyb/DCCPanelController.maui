using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : Entity, ITextEntity, IDrawingEntity {

    [ObservableProperty] [property: EditableString("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableInt("Font Size", "", 1, "Text")]
    private int _fontSize = 15;

    [ObservableProperty] [property: EditableColor("Text Color", "", 2, "Text")]
    private Color _textColor = Colors.White;

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

    [ObservableProperty] [property: EditableDouble("Scale", "", 5, "Circle", .25, 2.0)]
    private double _scale = 0.8;


    [JsonConstructor]
    public CircleLabelEntity() { }

    [JsonIgnore] protected override int RotationFactor => 90;

    public CircleLabelEntity(Panel panel) : this() {
        Parent = panel;
        Layer = 8;
    }

    public CircleLabelEntity(CircleLabelEntity entity) : base(entity) { }

    public override string EntityName => "Circle Label";

    public override Entity Clone() {
        return new CircleLabelEntity(this);
    }
}