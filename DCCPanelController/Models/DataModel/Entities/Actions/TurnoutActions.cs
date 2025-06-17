using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Services;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class TurnoutActions : ObservableCollection<TurnoutAction>, ICloneable {
    public TurnoutActions() { }

    public TurnoutActions(TurnoutActions buttonActions) {
        foreach (var action in buttonActions) Add(new TurnoutAction(action));
    }

    public async void Apply(TurnoutEntity turnout, ConnectionService connectionService, ActionExecutionContext context) {
        Console.WriteLine($"Applying actions to turnout {turnout.TurnoutID} with state {turnout.State}");

        foreach (var action in turnout.ButtonPanelActions) {
            if (turnout.Parent?.GetButtonEntity(action.Id) is { } actionButton) {
                var newState = turnout.State switch {
                    TurnoutStateEnum.Closed => action.WhenOn,
                    TurnoutStateEnum.Thrown => action.WhenOff,
                    _                       => ButtonStateEnum.Unknown
                };
                actionButton.SetState(newState, StateChangeSource.Internal, context);
            }
        }

        foreach (var action in turnout.TurnoutPanelActions) {
            if (turnout.Parent?.GetTurnoutEntity(action.Id) is { } actionTurnout) {
                var newState = turnout.State switch {
                    TurnoutStateEnum.Closed => action.WhenClosed,
                    TurnoutStateEnum.Thrown => action.WhenThrown,
                    _                       => TurnoutStateEnum.Unknown
                };
                actionTurnout.SetState(newState, StateChangeSource.Internal, context);
                if (connectionService.Client is { } client && actionTurnout.Turnout != null) {
                    await client.SendTurnoutCmdAsync(actionTurnout.Turnout, newState != TurnoutStateEnum.Closed);
                }
            }
        }
    }

    public object Clone() {
        var turnoutActions = new TurnoutActions();
        foreach (var action in this) turnoutActions.Add(new TurnoutAction(action));
        return turnoutActions;
    }
}

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private TurnoutStateEnum _whenClosed = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _whenThrown = TurnoutStateEnum.Unknown;

    public TurnoutAction() { }

    public TurnoutAction(TurnoutAction action) {
        Id = action.Id;
        WhenClosed = action.WhenClosed;
        WhenThrown = action.WhenThrown;
    }
}