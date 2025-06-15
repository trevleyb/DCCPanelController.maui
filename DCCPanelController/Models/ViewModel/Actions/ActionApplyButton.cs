// using DccClients.Jmri.Helpers;
// using DCCPanelController.Models.DataModel;
// using DCCPanelController.Models.DataModel.Entities;
// using DCCPanelController.Services;
//
// namespace DCCPanelController.Models.ViewModel.Actions;
//
// public static class ActionApplyButton {
//     // APPLY ACTIONS: There are two variations - one to change related buttons and one to change turnouts
//     //                When we action from here, we are in the context of the state of a BUTTON, and so 
//     //                we set the other buttons or turnouts based on the state of this button only. 
//
//     public static void ApplyButtonActions(ButtonEntity button, ButtonStateEnum state, ConnectionService connectionService) {
//
//         var panel = button.Parent;
//         var actions = button.ButtonPanelActions;
//
//         Console.WriteLine($"Applying actions to button {button.Id} with state {state}");
//         
//         foreach (var action in button.ButtonPanelActions) {
//             var actionButton = panel?.GetButtonEntity(action.Id);
//             if (actionButton is null) continue;
//
//             // Get what state we should be setting the related button to
//             // -----------------------------------------------------------------
//             var buttonState = button.State switch {
//                 ButtonStateEnum.On  => action.WhenOn,
//                 ButtonStateEnum.Off => action.WhenOff,
//                 _                   => ButtonStateEnum.Unknown // Ignore an Unknown State
//             };
//             Console.WriteLine($"Applying action {action.Id} {button.State} to button {actionButton.Id} with state {buttonState}");
//             // TODO: Fix
//             // actionButton.SetButtonState(buttonState);
//             if (action.Cascade) {
//                 Console.WriteLine($"Cascading action {action.Id} to {actionButton.Id}");
//                 ApplyButtonActions(actionButton, buttonState, connectionService);
//             }
//         }
//
//         foreach (var action in button.TurnoutPanelActions) {
//             var actionTurnout = panel?.GetTurnoutEntity(action.Id);
//             if (actionTurnout is null) continue;
//
//             // Get what state we should be setting the related button to
//             // -----------------------------------------------------------------
//             var turnoutState = button.State switch {
//                 ButtonStateEnum.On  => action.WhenClosed,
//                 ButtonStateEnum.Off => action.WhenThrown,
//                 _                   => TurnoutStateEnum.Unknown // Ignore an Unknown State
//             };
//             Console.WriteLine($"Applying action {action.Id} {button.State} to button {actionTurnout.TurnoutID} with state {turnoutState}");
//             
//             // TODO: Fix
//             // actionTurnout.SetTurnoutState(turnoutState);
//             if (action.Cascade) {
//                 Console.WriteLine($"Cascading action {action.Id} to {actionTurnout.TurnoutID}");
//                 //ActionApplyTurnout.ApplyTurnoutActions(action, buttonState, connectionService);
//             }
//         }
//     }
// }