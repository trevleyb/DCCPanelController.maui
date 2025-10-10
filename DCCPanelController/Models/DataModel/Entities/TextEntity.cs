using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.BaseClasses;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TextEntity : DrawingEntity, ITextEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Background Color", "", 2, "Color")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: Editable("Font Size", "", 2, "Text")]
    private int _fontSize = 12;

    [ObservableProperty] [property: Editable("Font Style", "", 1, "Text", EditorKind = EditorKinds.FontAlias)]
    private string _fontAlias = FontCatalog.DefaultFontAlias;
    
    [ObservableProperty] [property: Editable("Horizontal", "", 3, "Text")]
    private TextAlignmentHorizontalEnum _horizontalJustification = TextAlignmentHorizontalEnum.Center;

    [ObservableProperty] [property: Editable("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: Editable("Text Color", "", 1, "Color")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: Editable("Vertical", "", 3, "Text")]
    private TextAlignmentVerticalEnum _verticalJustification = TextAlignmentVerticalEnum.Center;

    [JsonConstructor]
    public TextEntity() { }

    public TextEntity(Panel panel) : this() => Parent = panel;
    public TextEntity(TextEntity entity) : base(entity) { }
    
    [JsonIgnore] protected override int RotationFactor => 45;

    [JsonIgnore] public override string EntityName => "Text";
    [JsonIgnore] public override string EntityDescription => "Text Label";
    [JsonIgnore]  public override string EntityInformation =>
        "The Text Tile is a label that can be used to display text on the layout. You can size it using ther size tool and set the color and opacity." ;

    public override void RotateLeft() {
        base.RotateLeft();
    }

    public override void RotateRight() {
        base.RotateRight();
    }

    public override Entity Clone() => new TextEntity(this);
}