using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TurnoutButtonEntity : Entity, IEntityGeneratingID, IInteractiveEntity {

    [ObservableProperty] [property: EditableID("Button Name","Unique Name for this Button so it can be referenced by actions.",0)]
    private string _id = string.Empty;

    [ObservableProperty] [property: EditableEnum("Button Size","",1)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: EditableColor("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: EditableColor("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;
    
    [ObservableProperty] [property: EditableColor("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;
    
    [ObservableProperty] [property: EditableColor("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: EditableTurnout("Turnout", "Turnout to Control with this button", 5, "Turnout")]
    private string? _turnout;

    [ObservableProperty] [property: EditableButton("When Straight/Normal", "When Turnout is Straight/Thrown set Button to?", 8, "Turnout")]
    private ButtonStateEnum _whenNormal;

    [ObservableProperty] [property: EditableButton("When Diverging/Thrown", "When Turnout is Diverging/Thrown set Button to?", 8, "Turnout")]
    private ButtonStateEnum _whenThrown;

    [ObservableProperty] 
    private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [JsonIgnore] protected override int RotationFactor => 90;
    [JsonIgnore] public List<IEntityID> AllIDs => new List<IEntityID>(Parent?.GetAllEntitiesByType<ActionButtonEntity>() ?? []) ?? [];
    [JsonIgnore] public string NextID => EntityID.GenerateNextID(Parent?.GetAllEntitiesByType<ActionButtonEntity>() ?? [],"Button");

    [JsonConstructor]
    public TurnoutButtonEntity() {
        Id = NextID;
    }

    public TurnoutButtonEntity(Panel panel) : base(panel) { }
    public TurnoutButtonEntity(TurnoutButtonEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") { }
    public override string EntityName => "Turnout Button";

    public override Entity Clone() {
        return new TurnoutButtonEntity(this);
    }
}