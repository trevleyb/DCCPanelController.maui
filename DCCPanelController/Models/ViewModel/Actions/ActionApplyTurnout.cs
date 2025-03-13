using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Actions;

public static class ActionApplyTurnout {
    // APPLY ACTIONS: There are two variations - one to change related buttons and one to change turnouts
    //                When we action from here, we are in the context of the state of a BUTTON, and so 
    //                we set the other buttons or turnouts based on the state of this button only. 

    public static void ApplyTurnoutActions(Panel panel, TurnoutEntity turnout, ActionList actionsList) {
        foreach (var action in turnout.ButtonActions) {
            var actionButton = panel.GetButton(action.Id);
            if (actionButton is null) continue;
            if (actionsList.IsActioned(ActionType.Button, actionButton.Id)) continue;

            // Get what state we should be setting the related turnout to
            // -----------------------------------------------------------------
            var buttonState = turnout.State switch {
                TurnoutStateEnum.Closed => action.WhenOnOrClosed,
                TurnoutStateEnum.Thrown => action.WhenOffOrThrown,
                _                       => ButtonStateEnum.Unknown // Ignore an Unknown State
            };

            // TODO: FIX THIS
            // actionButton.SetButtonState(buttonState);
            // if (action.Cascade) actionButton.ExecButtonState(buttonState, actionsList);
        }

        foreach (var action in turnout.TurnoutActions) {
            var actionTurnout = panel.GetTurnout(action.Id);
            if (actionTurnout is null) continue;
            if (actionsList.IsActioned(ActionType.Turnout, actionTurnout.Id)) continue;

            // Get what state we should be setting the related turnout to
            // -----------------------------------------------------------------
            var turnoutState = turnout.State switch {
                TurnoutStateEnum.Closed => action.WhenOnOrClosed,
                TurnoutStateEnum.Thrown => action.WhenOffOrThrown,
                _                       => TurnoutStateEnum.Unknown // Ignore an Unknown State
            };

            // TODO: FIX THIS
            // actionTurnout.SetTurnoutState(turnoutState);
            // if (action.Cascade) actionTurnout.ExecTurnoutState(turnoutState, actionsList);
        }
    }
}