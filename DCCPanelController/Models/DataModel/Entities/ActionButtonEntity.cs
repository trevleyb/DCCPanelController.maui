using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Properties.DynamicProperties;
using DCCPanelController.View.TileSelectors;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ActionButtonEntity : Entity, IEntityGeneratingID, IInteractiveEntity, IActionEntity {

    [ObservableProperty]
    private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: Editable("Button Name", "Unique Name for this Button so it can be referenced by actions.", Order=1)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Button Size", Order=2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Off Color", "Override default 'Off' color", 5, Group="Colors")]
    private Color? _colorOff;

    [ObservableProperty] [property: Editable("Off Border Color", "Override default 'Off' border color", 5, Group="Colors")]
    private Color? _colorOffBorder;

    [ObservableProperty] [property: Editable("On Color", "Override default 'On' color", 5, "Colors")]
    private Color? _colorOn;

    [ObservableProperty] [property: Editable("On Border Color", "Override default 'On' border color", 5, Group="Colors")]
    private Color? _colorOnBorder;

    [ObservableProperty] [property: Editable("Button Actions", Order=10, Group="Actions", EditorKind = EditorKinds.ButtonActions)]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: Editable("Turnout Actions", Order=10, Group="Actions", EditorKind = EditorKinds.TurnoutActions)]
    private TurnoutActions _turnoutPanelActions = [];

    [JsonConstructor]
    public ActionButtonEntity() {
        Id = NextID;
    }

    public ActionButtonEntity(Panel panel) : base(panel) { }
    public ActionButtonEntity(ActionButtonEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") { }

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "A-Button";
    public override string EntityDescription => "Trigger Actions Button";

    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)TurnoutPanelActions.Clone();
    }

    [JsonIgnore] public List<IEntityID> AllIDs => new List<IEntityID>(Parent?.GetAllEntitiesByType<ActionButtonEntity>() ?? []) ?? [];
    [JsonIgnore] public string NextID => EntityID.GenerateNextID(Parent?.GetAllEntitiesByType<ActionButtonEntity>() ?? [], "Button");

    public override Entity Clone() {
        return new ActionButtonEntity(this);
    }

    public void SetState(ButtonStateEnum newState, StateChangeSource source, ActionExecutionContext? context = null) {
        if (State == newState) return;

        State = newState;

        // Only trigger cascading if this is an external change or we're not already cascading this entity
        if (source == StateChangeSource.External || context?.CanCascade(Id) == true) {
            context ??= new ActionExecutionContext();
            using (context.BeginCascade(Id)) {
                ButtonPanelActions.Apply(this, ConnectionService.Instance, context);
            }
        }
    }
}