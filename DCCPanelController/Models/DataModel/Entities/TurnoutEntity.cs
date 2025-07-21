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
    // [ObservableProperty] [property: EditableID("Turnout Name", "Unique name for this Turnout", 0, "Turnout")]
    // private string _id = string.Empty;
  
    [ObservableProperty] [property: EditableTurnout("DCC Turnout", "Turnout ID on the layout that will be controlled.", 0, "Turnout")]
    private string _id = string.Empty;

    [ObservableProperty] [property: EditableEnum("Turnout Style", "Standard shows the branching route. ", 4, "Track")]
    private TurnoutStyleEnum _turnoutStyle = TurnoutStyleEnum.Standard;

    [ObservableProperty] [property: EditableColor("Not Selected Track", "The color of the track of the track not selected", 6, "Track")]
    private Color? _trackNotSelectedColor;
    
    [ObservableProperty] [property: EditableButtonActions("Button Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: EditableTurnoutActions("Turnout Actions", "", 10, "Actions", ActionsContext.Turnout)]
    private TurnoutActions _turnoutPanelActions = [];

    [ObservableProperty]
    private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;

    [JsonConstructor]
    protected TurnoutEntity() { }
    
    [JsonIgnore]
    public Turnout? Turnout => Parent?.Turnout(Id);

    protected TurnoutEntity(Panel panel) : base(panel) { }

    [JsonIgnore] protected override int RotationFactor => 90;
    [JsonIgnore] public List<IEntityID> AllIDs => new List<IEntityID>(Parent?.GetAllEntitiesByType<TurnoutEntity>() ?? []) ?? [];
    [JsonIgnore] public string NextID => EntityID.GenerateNextID(Parent?.GetAllEntitiesByType<TurnoutEntity>() ?? [],"Turnout");

    protected TurnoutEntity(TurnoutEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") {
        ButtonPanelActions = new ButtonActions(entity.ButtonPanelActions);
        TurnoutPanelActions = new TurnoutActions(entity.TurnoutPanelActions);
    }

    public override string ToString() {
        return Id;
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

    /// <summary>
    /// This function is design to find the entity that the diverging track on
    /// this turnout would connect to. We can do this so that if it is a branch
    /// line and the turnout is a mainline, then we can hide the border. 
    /// </summary>
    /// <returns>The neighbor Entity</returns>
    public Entity? GetDivergingEntity() {
        var connections = Connections.GetConnections(Rotation);
        for (int i = 0; i <= connections.Length; i++) {
            if (connections[i] == ConnectionType.Diverging) {
                var neighborOffset = EntityConnections.GetDirectionOffset(i);
                var neighborCol = Col + neighborOffset.dx;
                var neighborRow = Row + neighborOffset.dy;
                var neighbor = Parent?.GetEntityAtPosition(neighborCol, neighborRow);
                return neighbor;
            }
        }
        return null;
    }
    
}