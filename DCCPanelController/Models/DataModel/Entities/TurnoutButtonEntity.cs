using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TurnoutButtonEntity : Entity, IEntityGeneratingID, IInteractiveEntity {

    [ObservableProperty] [property: Editable("Button Name","Unique Name for this Button so it can be referenced by actions.",0)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Button Size","",1)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("On Border Color", "Override default 'On' border color", 5, "Colors")]
    private Color? _colorOnBorder;
    
    [ObservableProperty] [property: Editable("Off Color", "Override default 'Off' color", 5, "Colors")]
    private Color? _colorOff;
    
    [ObservableProperty] [property: Editable("Off Border Color", "Override default 'Off' border color", 5, "Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: Editable("Turnout", "Turnout to Control with this button", 5, "Turnout", EditorKind = EditorKinds.Turnout)]
    private string? _turnout;

    [ObservableProperty] [property: Editable("Straight/Normal", "When Turnout is Straight/Thrown set Button to?", 8, "Turnout", EditorKind = EditorKinds.ButtonState)]
    private ButtonStateEnum _whenNormal;

    [ObservableProperty] [property: Editable("Diverging/Thrown", "When Turnout is Diverging/Thrown set Button to?", 8, "Turnout", EditorKind = EditorKinds.ButtonState)]
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
    public override string EntityName => "T-Button";
    public override string EntityDescription => "Turnout Toggle Switch";
    public override Entity Clone() {
        return new TurnoutButtonEntity(this);
    }
}