using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.DynamicProperties;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : TrackEntity, IInteractiveEntity, ITrackEntity, IActionEntity, IEntityID {

    public string Id { get; set; } = string.Empty;

    [ObservableProperty]
    private TurnoutStateEnum _state = TurnoutStateEnum.Unknown; // We should not need to know this

    private StateChangeSource _stateChangeSource = StateChangeSource.External;

    [ObservableProperty] [property: Editable("Not Selected Track", "The color of the track of the track not selected", 6, "Color")]
    private Color? _trackNotSelectedColor;

    [ObservableProperty] [property: Editable("DCC Turnout", "Turnout ID on the layout that will be controlled.", 5, "General", EditorKind = EditorKinds.Turnout)]
    private string _turnoutID = string.Empty;

    [ObservableProperty] [property: Editable("Turnout Actions", "Sets Turnouts based on the state of this turnout", 10, "Turnout Actions")]
    private TurnoutActions _turnoutPanelActions = [];

    [ObservableProperty] [property: Editable("Button Actions", "Sets Buttons based on the state of this Turnout", 10, "Button Actions")]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: Editable("Turnout Style", "Standard shows the branching route. ", 4, "Track")]
    private TurnoutStyleEnum _turnoutStyle = TurnoutStyleEnum.Standard;

    [JsonConstructor]
    protected TurnoutEntity() { }
    protected TurnoutEntity(Panel panel) : base(panel) { }
    protected TurnoutEntity(TurnoutEntity entity) : base(entity) { }

    [JsonIgnore]
    public Turnout? Turnout => Parent?.Turnout(TurnoutID);

    [JsonIgnore] protected override int RotationFactor => 45;

    public ActionsContext Context => ActionsContext.Turnout;

    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)TurnoutPanelActions.Clone();
    }

    public string NextID(Panel? targetPanel = null) {
        targetPanel ??= Parent;
        var nextID = EntityHelper.GenerateID(EntityHelper.GetAllEntitiesByType<TurnoutEntity>(targetPanel), "Turnout");
        return nextID;
    }

    public override string ToString() => TurnoutID;

    public void SetState(TurnoutStateEnum newState, StateChangeSource source, ActionExecutionContext? context = null) {
        if (State == newState) return;

        _stateChangeSource = source;
        State = newState;
        Turnout?.State = State;
        
        // Only trigger cascading if this is an external change or we're not already cascading this entity
        if (source == StateChangeSource.External || context?.CanCascade(TurnoutID) == true) {
            context ??= new ActionExecutionContext();
            using (context.BeginCascade(Id)) {
                var task =TurnoutPanelActions.ApplyAsync(this, ConnectionService.Instance, context);
                _ = task.ContinueWith(t => {
                    LogHelper.Logger.LogError(t.Exception, "TurnoutActions.ApplyAsync failed");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }

        }
    }

    public Entity? GetNeighbourEntity(ConnectionType connectionType) {
        var connections = Connections.GetConnections(Rotation);
        for (var i = 0; i <= connections.Length; i++) {
            if (connections[i] == connectionType) {
                var neighborOffset = EntityConnections.GetDirectionOffset(i);
                var neighborCol = Col + neighborOffset.dx;
                var neighborRow = Row + neighborOffset.dy;
                var neighbor = Parent?.GetEntityAtPosition(neighborCol, neighborRow);
                return neighbor;
            }
        }
        return null;
    }
    
    /// <summary>
    ///     This function is design to find the entity that the diverging track on
    ///     this turnout would connect to. We can do this so that if it is a branch
    ///     line and the turnout is a mainline, then we can hide the border.
    /// </summary>
    /// <returns>The neighbor Entity</returns>
    public Entity? GetDivergingEntity() => GetNeighbourEntity(ConnectionType.Diverging);

}