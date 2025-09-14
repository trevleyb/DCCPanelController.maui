using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ActionButtonEntity : ButtonEntity, IEntityGeneratingID, IInteractiveEntity, IActionEntity {

    [ObservableProperty] [property: Editable("Button Size", Order = 2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Button Name", "Unique Name for this Button so it can be referenced by actions.", Order = 1)]
    private string _id = string.Empty;

    [ObservableProperty]
    private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: Editable("Button Actions", "Sets Buttons based on the state of this Button" , Order = 10, Group = "Button Actions", EditorKind = EditorKinds.ButtonActions)]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: Editable("Turnout Actions", "Sets Turnouts based on the state of this Button", Order = 10, Group = "Turnout Actions", EditorKind = EditorKinds.TurnoutActions)]
    private TurnoutActions _turnoutPanelActions = [];

    [JsonConstructor]
    public ActionButtonEntity() => Id = NextID();

    public ActionButtonEntity(Panel panel) : base(panel) { }
    public ActionButtonEntity(ActionButtonEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") { }

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "A-Button";
    public override string EntityDescription => "Trigger Actions Button";

    public override string EntityInformation =>
        "This button allows you to trigger actions on the click of the **button**. " +
        "These actions can include setting other __buttons__, turning on and off __lights__ or switches, " +
        "triggering a __route__, or throwing a __turnout__. When other __buttons__ or __turnouts__ are triggered " +
        "by this button, the actions will cascade down to the __buttons__ and __turnouts__ that are connected to it.";

    public ActionsContext Context => ActionsContext.Button;

    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)TurnoutPanelActions.Clone();
    }

    public string NextID(Panel? targetPanel = null) {
        targetPanel ??= Parent;
        var nextID = EntityHelper.GenerateID(EntityHelper.GetAllEntitiesByType<ActionButtonEntity>(targetPanel), "Button");
        return nextID;
    }

    public override Entity Clone() => new ActionButtonEntity(this);

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