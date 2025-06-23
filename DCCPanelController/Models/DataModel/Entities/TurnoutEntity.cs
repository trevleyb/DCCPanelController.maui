using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : TrackEntity, IEntityID, IInteractiveEntity, ITrackEntity, IActionEntity {
    [ObservableProperty] [property: EditableID("Turnout Name", "Unique name for this Turnout", 0, "Turnout")]
    private EntityIDField _id = string.Empty;

    [ObservableProperty] [property: EditableTurnout("DCC Turnout", "Turnout ID on the layout that will be controlled.", 0, "Turnout")]
    private string _turnoutID = string.Empty;

    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private TurnoutActions _turnoutPanelActions = [];

    [ObservableProperty]
    private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;

    [JsonConstructor]
    protected TurnoutEntity() {
        Id = NextID;
    }
    
    [JsonIgnore]
    public Turnout? Turnout => Parent?.Turnout(TurnoutID);

    protected TurnoutEntity(Panel panel) : base(panel) { }

    [JsonIgnore] public List<IEntityID> AllIDs => new List<IEntityID>(Parent?.GetAllEntitiesByType<TurnoutEntity>() ?? []) ?? [];
    [JsonIgnore] public string NextID => EntityID.GenerateNextID(Parent?.GetAllEntitiesByType<TurnoutEntity>() ?? [],"Turnout");

    protected TurnoutEntity(TurnoutEntity entity) : base(entity) {
        TurnoutID = string.Empty;
        ButtonPanelActions = new ButtonActions(entity.ButtonPanelActions);
        TurnoutPanelActions = new TurnoutActions(entity.TurnoutPanelActions);
        State = TurnoutStateEnum.Unknown;
        TrackColor = entity.TrackColor;
        RotationFactor = 90;
    }

    public override string ToString() {
        return TurnoutID;
    }
    
    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)this.ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)this.TurnoutPanelActions.Clone();
    }
    
    private StateChangeSource _stateChangeSource = StateChangeSource.External;
    public void SetState(TurnoutStateEnum newState, StateChangeSource source, ActionExecutionContext? context = null) {
        if (State == newState) return;
        
        _stateChangeSource = source;
        State = newState;
        
        // Only trigger cascading if this is an external change or we're not already cascading this entity
        if (source == StateChangeSource.External || (context?.CanCascade(Id) == true)) {
            context ??= new ActionExecutionContext();
            using (context.BeginCascade(Id)) {
                TurnoutPanelActions.Apply(this, ConnectionService.Instance, context);
            }
        }
    }

}