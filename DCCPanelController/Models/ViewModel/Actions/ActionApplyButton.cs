using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Actions;

public static class ActionApplyButton {
    // APPLY ACTIONS: There are two variations - one to change related buttons and one to change turnouts
    //                When we action from here, we are in the context of the state of a BUTTON, and so 
    //                we set the other buttons or turnouts based on the state of this button only. 

    public static void ApplyButtonActions(Panel panel, ButtonEntity button, ActionList actionsList) {
        foreach (var action in button.ButtonActions) {
            var actionButton = panel.GetButton(action.Id);
            if (actionButton is null) continue;
            if (actionsList.IsActioned(ActionType.Button, actionButton.Id)) continue;

            // Get what state we should be setting the related button to
            // -----------------------------------------------------------------
            var buttonState = button.State switch {
                ButtonStateEnum.On   => action.WhenOnOrClosed,
                ButtonStateEnum.Off => action.WhenOffOrThrown,
                _                        => ButtonStateEnum.Unknown // Ignore an Unknown State
            };

            // TODO: Fix
            // actionButton.SetButtonState(buttonState);
            // if (action.Cascade) actionButton.ExecButtonState(buttonState, actionsList);
        }

        foreach (var action in button.TurnoutActions) {
            var actionTurnout = panel.GetTurnout(action.Id);
            if (actionTurnout is null) continue;
            if (actionsList.IsActioned(ActionType.Turnout, actionTurnout.Id)) continue;

            // Get what state we should be setting the related button to
            // -----------------------------------------------------------------
            var turnoutState = button.State switch {
                ButtonStateEnum.On  => action.WhenOnOrClosed,
                ButtonStateEnum.Off => action.WhenOffOrThrown,
                _                        => TurnoutStateEnum.Unknown // Ignore an Unknown State
            };

            // TODO: Fix
            // actionTurnout.SetTurnoutState(turnoutState);
            // if (action.Cascade) actionTurnout.ExecTurnoutState(turnoutState, actionsList);
        }
    }
}