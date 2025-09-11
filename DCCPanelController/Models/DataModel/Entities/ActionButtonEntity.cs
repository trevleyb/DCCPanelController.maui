using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ActionButtonEntity : ButtonEntity, IEntityGeneratingID, IInteractiveEntity, IActionEntity {

    [ObservableProperty]
    private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: Editable("Button Name", "Unique Name for this Button so it can be referenced by actions.", Order=1)]
    private string _id = string.Empty;

    [ObservableProperty] [property: Editable("Button Size", Order=2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Button Actions", Order=10, Group="Actions", EditorKind = EditorKinds.ButtonActions)]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: Editable("Turnout Actions", Order=10, Group="Actions", EditorKind = EditorKinds.TurnoutActions)]
    private TurnoutActions _turnoutPanelActions = [];

    [JsonConstructor]
    public ActionButtonEntity() {
        Id = NextID();
    }

    public ActionButtonEntity(Panel panel) : base(panel) { }
    public ActionButtonEntity(ActionButtonEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") { }

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "A-Button";
    public override string EntityDescription => "Trigger Actions Button";

    public override string EntityInformation => 
        "This button allows you to trigger actions on the click of the button. " +
        "These actions can include setting other __buttons__, turning on and off __lights__ or switches, " +
        "triggering a __route__, or throwing a __turnout__. When other __buttons__ or __turnouts__ are triggered " +
        "by this button, the actions will cascade down to the __buttons__ and __turnouts__ that are connected to it."; 
    
    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)TurnoutPanelActions.Clone();
    }

    /// <summary>
    /// Because when we are editing a panel the entities do not actually exist UNTIL we save,
    /// we need to get all saved IDs from the parent AND we need to get any which are in the
    /// local panel (unsaved) combine them and use that list to generate a unique ID.
    /// </summary>
    public List<IEntityID> AllIDs() {
        var allIDs = new List<IEntityID>(Parent?.GetAllEntitiesByType<ActionButtonEntity>() ?? []) ?? [];
        var localIDs = new List<IEntityID>(Parent?.GetPanelEntitiesByType<ActionButtonEntity>() ?? []) ?? [];
        var availableIDs = localIDs.Union(allIDs).ToList();
        return availableIDs;
    }

    public string NextID() {
        var nextID = EntityHelper.GenerateID(AllIDs() ?? [], "Button");
        return nextID;
    }

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