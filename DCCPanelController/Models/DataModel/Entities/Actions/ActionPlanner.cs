using System.Collections.Generic;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public static class ActionPlanner {
    // From a BUTTON change, gather all follow-on actions without mutating state.
    public static List<PlannedAction> PlanForButtonChange(ActionButtonEntity button) {
        var plan = new List<PlannedAction>();

        // Buttons that this button controls
        foreach (var action in button.ButtonPanelActions) {
            if (button.Parent?.GetButtonEntity(action.ActionID) is { } target) {
                var newState = button.State switch {
                    ButtonStateEnum.On  => action.WhenOn,
                    ButtonStateEnum.Off => action.WhenOff,
                    _                   => ButtonStateEnum.Unknown
                };
                if (newState != ButtonStateEnum.Unknown)
                    plan.Add(new PlannedAction(ActionTargetKind.Button, target.Id, target, newState));
            }
        }

        // Turnouts that this button controls
        foreach (var action in button.TurnoutPanelActions) {
            if (button.Parent?.GetTurnoutEntity(action.ActionID) is { } target) {
                var newState = button.State switch {
                    ButtonStateEnum.On  => action.WhenClosed,
                    ButtonStateEnum.Off => action.WhenThrown,
                    _                   => TurnoutStateEnum.Unknown
                };
                if (newState != TurnoutStateEnum.Unknown)
                    plan.Add(new PlannedAction(ActionTargetKind.Turnout, target.Id, target, newState));
            }
        }

        return plan;
    }

    // From a TURNOUT change, gather all follow-on actions without mutating state.
    public static List<PlannedAction> PlanForTurnoutChange(TurnoutEntity turnout) {
        var plan = new List<PlannedAction>();

        // Buttons that mirror this turnout
        foreach (var action in turnout.ButtonPanelActions) {
            if (turnout.Parent?.GetButtonEntity(action.ActionID) is { } target) {
                var newState = turnout.State switch {
                    TurnoutStateEnum.Closed => action.WhenOn,
                    TurnoutStateEnum.Thrown => action.WhenOff,
                    _                       => ButtonStateEnum.Unknown
                };
                if (newState != ButtonStateEnum.Unknown)
                    plan.Add(new PlannedAction(ActionTargetKind.Button, target.Id, target, newState));
            }
        }

        // Other turnouts this turnout controls
        foreach (var action in turnout.TurnoutPanelActions) {
            if (turnout.Parent?.GetTurnoutEntity(action.ActionID) is { } target) {
                var newState = turnout.State switch {
                    TurnoutStateEnum.Closed => action.WhenClosed,
                    TurnoutStateEnum.Thrown => action.WhenThrown,
                    _                       => TurnoutStateEnum.Unknown
                };
                if (newState != TurnoutStateEnum.Unknown)
                    plan.Add(new PlannedAction(ActionTargetKind.Turnout, target.Id, target, newState));
            }
        }

        return plan;
    }
}