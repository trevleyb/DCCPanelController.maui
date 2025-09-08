using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.DynamicProperties;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TurnoutEntity : TrackEntity, IInteractiveEntity, ITrackEntity, IActionEntity {
   
    [ObservableProperty]
    private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;

    private StateChangeSource _stateChangeSource = StateChangeSource.External;

    [ObservableProperty] [property: Editable("Not Selected Track", "The color of the track of the track not selected", 6, "Track")]
    private Color? _trackNotSelectedColor;

    // [ObservableProperty] [property: EditableID("Turnout Name", "Unique name for this Turnout", 0, "Turnout")]
    // private string _id = string.Empty;

    [ObservableProperty] [property: Editable("DCC Turnout", "Turnout ID on the layout that will be controlled.", 0, "Turnout", EditorKind = EditorKinds.Turnout)]
    private string _turnoutID = string.Empty;

    [ObservableProperty] [property: Editable("Button Actions", "", 10, "Actions", ActionsContext = ActionsContext.Turnout)]
    private ButtonActions _buttonPanelActions = [];

    [ObservableProperty] [property: Editable("Turnout Actions", "", 10, "Actions", ActionsContext = ActionsContext.Turnout)]
    private TurnoutActions _turnoutPanelActions = [];

    [ObservableProperty] [property: Editable("Turnout Style", "Standard shows the branching route. ", 4, "Track")]
    private TurnoutStyleEnum _turnoutStyle = TurnoutStyleEnum.Standard;

    [JsonConstructor]
    protected TurnoutEntity() { }

    protected TurnoutEntity(Panel panel) : base(panel) { }

    protected TurnoutEntity(TurnoutEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") {
        ButtonPanelActions = new ButtonActions(entity.ButtonPanelActions);
        TurnoutPanelActions = new TurnoutActions(entity.TurnoutPanelActions);
    }

    [JsonIgnore]
    public Turnout? Turnout => Parent?.Turnout(TurnoutID);

    [JsonIgnore] protected override int RotationFactor => 90;

    public void CloneActionsInto(IActionEntity entity) {
        entity.ButtonPanelActions = (ButtonActions)ButtonPanelActions.Clone();
        entity.TurnoutPanelActions = (TurnoutActions)TurnoutPanelActions.Clone();
    }

    public override string ToString() {
        return TurnoutID;
    }

    public void ToggleState() {
        var newState = State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };
        SetState(newState, _stateChangeSource);
    }

    public void SetState(TurnoutStateEnum newState, StateChangeSource source, ActionExecutionContext? context = null) {
        if (State == newState) return;

        _stateChangeSource = source;
        State = newState;

        // Only trigger cascading if this is an external change or we're not already cascading this entity
        if (source == StateChangeSource.External || context?.CanCascade(TurnoutID) == true) {
            context ??= new ActionExecutionContext();
            using (context.BeginCascade(TurnoutID)) {
                TurnoutPanelActions.Apply(this, ConnectionService.Instance, context);
            }
        }
    }

    /// <summary>
    ///     This function is design to find the entity that the diverging track on
    ///     this turnout would connect to. We can do this so that if it is a branch
    ///     line and the turnout is a mainline, then we can hide the border.
    /// </summary>
    /// <returns>The neighbor Entity</returns>
    public Entity? GetDivergingEntity() {
        var connections = Connections.GetConnections(Rotation);
        for (var i = 0; i <= connections.Length; i++) {
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