using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : Entity, ITextEntity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Background Color", "", 5, "Border")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableInt("Font Size", "", 1, "Text")]
    private int _fontSize = 8;

    [ObservableProperty] [property: EditableEnum("Horizontal", "", 3, "Text")]
    private TextAlignmentHorizontalEnum _horizontalJustification = TextAlignmentHorizontalEnum.Center;

    [ObservableProperty] [property: EditableString("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableColor("Text Color", "", 2, "Text")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: EditableEnum("Vertical", "", 3, "Text")]
    private TextAlignmentVerticalEnum _verticalJustification = TextAlignmentVerticalEnum.Center;

    [JsonConstructor]
    public TextEntity() { }

    public TextEntity(Panel panel) : this() {
        Parent = panel;
    }

    public TextEntity(TextEntity entity) : base(entity) { }

    //[ObservableProperty] [property: EditableColor("Border Color", "", 5, "Border")]
    //private Color _borderColor = Colors.Transparent;

    //[ObservableProperty] [property: EditableInt("Border Radius", "", 5, "Border")]
    //private int _borderRadius;

    //[ObservableProperty] [property: EditableInt("Border Width", "", 5, "Border")]
    //private int _borderWidth;

    [JsonIgnore] protected override int RotationFactor => 90;

    public override string EntityName => "Text Block";

    public override void RotateLeft() {
        base.RotateLeft();
        HandleRotation();
    }

    public override void RotateRight() {
        base.RotateRight();
        HandleRotation();
    }

    public override Entity Clone() {
        return new TextEntity(this);
    }
}