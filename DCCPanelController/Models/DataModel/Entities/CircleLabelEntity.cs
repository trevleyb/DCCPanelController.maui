using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.BaseClasses;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CircleLabelEntity : DrawingEntity, ITextEntity, IDrawingEntity {
    [ObservableProperty] [property: Editable("Background Color", "", 5, "Circle")]
    private Color _backgroundColor = Colors.DarkGray;

    [ObservableProperty] [property: Editable("Inner Border", "", 10, "Circle")]
    private Color _borderInnerColor = Colors.White;

    [ObservableProperty] [property: Editable("Outer Border", "", 11, "Circle")]
    private Color _borderOuterColor = Colors.Black;

    [ObservableProperty] [property: Editable("Gap Color", "", 12, "Circle")]
    private Color _gapColor = Colors.White;

    [ObservableProperty] [property: Editable("Inner Width", "", 13, "Circle")]
    private int _borderInnerWidth = 2;
    
    [ObservableProperty] [property: Editable("Outer Width", "", 14, "Circle")]
    private int _borderOuterWidth = 2;
    
    [ObservableProperty] [property: Editable("Gap Width", "", 15, "Circle")]
    private int _gapWidth = 2;



    [ObservableProperty] [property: Editable("Font Size", "", 1, "Text")]
    private int _fontSize = 15;

    [ObservableProperty] [property: Editable("Label", "", 0, "Text")]
    private string _label = string.Empty;

    [ObservableProperty] [property: Editable("Text Color", "", 2, "Text")]
    private Color _textColor = Colors.White;
    
    [ObservableProperty] [property: Editable("Font Style", "", 2, "Text", EditorKind = EditorKinds.FontAlias)]
    private string _fontAlias = FontCatalog.DefaultFontAlias;

    [JsonConstructor]
    public CircleLabelEntity() { }

    public CircleLabelEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CircleLabelEntity(CircleLabelEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 45;

    [JsonIgnore] public override string EntityName => "Label";
    [JsonIgnore] public override string EntityDescription => "Circle Label";
    [JsonIgnore] public override string EntityInformation =>
        "The Circle Label is a pre-defined drawable circle that supports an internal text block. This is often used to indicate turnout numbers or references on the layout.";

    public override Entity Clone() => new CircleLabelEntity(this);
}