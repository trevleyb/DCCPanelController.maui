using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : Entity, ITextEntity, IDrawingEntity {
    [ObservableProperty] [property: EditableColor("Background Color", "", 5, "Border")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableColor("Border Color", "", 5, "Border")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: EditableInt("Border Radius", "", 5, "Border")]
    private int _borderRadius;

    [ObservableProperty] [property: EditableInt("Border Width", "", 5, "Border")]
    private int _borderWidth;

    [ObservableProperty] [property: EditableInt("Font Size", "", 0, "Text")]
    private int _fontSize = 8;

    [ObservableProperty] [property: EditableEnum("Horizontal", "", 0, "Text")]
    private TextAlignmentHorizontalEnum _horizontalJustification = TextAlignmentHorizontalEnum.Center;

    [ObservableProperty] [property: EditableEnum("Vertical", "", 0, "Text")]
    private TextAlignmentVerticalEnum _verticalJustification = TextAlignmentVerticalEnum.Center;

    [ObservableProperty] [property: EditableString("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableColor("Text Color", "", 0, "Text")]
    private Color _textColor = Colors.Black;

    [JsonConstructor]
    public TextEntity() { }

    public TextEntity(Panel panel) : this() {
        Parent = panel;
    }

    public TextEntity(TextEntity entity) : base(entity) { }

    public override string EntityName => "Text Block";

    public override Entity Clone() {
        return new TextEntity(this);
    }
}