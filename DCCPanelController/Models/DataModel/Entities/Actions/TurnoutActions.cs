using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class TurnoutActions : ObservableCollection<TurnoutAction>, ICloneable {
    public TurnoutActions() { }

    public TurnoutActions(TurnoutActions buttonActions) {
        foreach (var action in buttonActions) Add(new TurnoutAction(action));
    }

    public object Clone() {
        var turnoutActions = new TurnoutActions();
        foreach (var action in this) turnoutActions.Add(new TurnoutAction(action));
        return turnoutActions;
    }

    public async void Apply(TurnoutEntity turnout, ConnectionService connectionService, ActionExecutionContext context) {
        var logger = LogHelper.CreateLogger("TurnoutActionsApply");
        try {
            foreach (var action in turnout.ButtonPanelActions) {
                if (turnout.Parent?.GetButtonEntity(action.ActionID) is { } actionButton) {
                    var newState = turnout.State switch {
                        TurnoutStateEnum.Closed => action.WhenOn,
                        TurnoutStateEnum.Thrown => action.WhenOff,
                        _                       => ButtonStateEnum.Unknown,
                    };
                    Console.WriteLine($"BUTTON(t): {turnout.Id} => {actionButton.Id} from {actionButton.State} to {newState}");;
                    if (newState != ButtonStateEnum.Unknown) {
                        //actionButton.SetState(newState, StateChangeSource.Internal, context);
                        actionButton.State = newState;
                    }
                }
            }

            foreach (var action in turnout.TurnoutPanelActions) {
                if (turnout.Parent?.GetTurnoutEntity(action.ActionID) is { } actionTurnout) {
                    var newState = turnout.State switch {
                        TurnoutStateEnum.Closed => action.WhenClosed,
                        TurnoutStateEnum.Thrown => action.WhenThrown,
                        _                       => TurnoutStateEnum.Unknown,
                    };
                    Console.WriteLine($"TURNOUT(t): {turnout.Id} => {actionTurnout.Id} from {actionTurnout.State} to {newState}");
                    if (newState != TurnoutStateEnum.Unknown) {
                        //actionTurnout.SetState(newState, StateChangeSource.Internal, context);
                        actionTurnout.State = newState;
                        if (connectionService.Client is { } client && actionTurnout.Turnout != null) {
                            await client.SendTurnoutCmdAsync(actionTurnout.Turnout, newState != TurnoutStateEnum.Closed);
                        }
                    }
                }
            }
        } catch (Exception ex) {
            logger.LogError("Error in Async Void function: TurnoutActions:Apply => {Message}", ex.Message);
        }
    }
}

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private string           _actionID   = string.Empty;
    [ObservableProperty] private TurnoutStateEnum _whenClosed = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _whenThrown = TurnoutStateEnum.Unknown;

    public TurnoutAction() { }

    public TurnoutAction(TurnoutAction action) {
        ActionID = action.ActionID;
        WhenClosed = action.WhenClosed;
        WhenThrown = action.WhenThrown;
    }
}