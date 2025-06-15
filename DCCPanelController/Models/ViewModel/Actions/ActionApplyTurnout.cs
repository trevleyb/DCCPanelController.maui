// using DCCPanelController.Models.DataModel;
// using DCCPanelController.Models.DataModel.Entities;
// using DCCPanelController.Services;
//
// namespace DCCPanelController.Models.ViewModel.Actions;
//
// public static class ActionApplyTurnout {
//     // APPLY ACTIONS: There are two variations - one to change related buttons and one to change turnouts
//     //                When we action from here, we are in the context of the state of a BUTTON, and so 
//     //                we set the other buttons or turnouts based on the state of this button only. 
//
//     public static void ApplyTurnoutActions(TurnoutEntity turnout, TurnoutStateEnum state, ConnectionService connectionService) {
//
//         var panel = turnout.Parent;
//         var actions = turnout.ButtonPanelActions;
//
//         foreach (var action in turnout.ButtonPanelActions) {
//             var actionButton = panel?.GetButtonEntity(action.Id);
//             if (actionButton is null) continue;
//
//             // Get what state we should be setting the related turnout to
//             // -----------------------------------------------------------------
//             var buttonState = turnout.State switch {
//                 TurnoutStateEnum.Closed => action.WhenOn,
//                 TurnoutStateEnum.Thrown => action.WhenOff,
//                 _                       => ButtonStateEnum.Unknown // Ignore an Unknown State
//             };
//             Console.WriteLine($"Applying action {action.Id} {turnout.State} to button {actionButton.Id} with state {buttonState}");
//             // TODO: FIX THIS
//             // actionButton.SetButtonState(buttonState);
//             if (action.Cascade) {
//                 Console.WriteLine($"Cascading action {action.Id} to {actionButton.Id}");
//                 //ApplyTurnoutActions(actionButton, buttonState, connectionService);
//             }
//         }
//
//         foreach (var action in turnout.TurnoutPanelActions) {
//             var actionTurnout = panel?.GetTurnoutEntity(action.Id);
//             if (actionTurnout is null) continue;
//
//             // Get what state we should be setting the related turnout to
//             // -----------------------------------------------------------------
//             var turnoutState = turnout.State switch {
//                 TurnoutStateEnum.Closed => action.WhenClosed,
//                 TurnoutStateEnum.Thrown => action.WhenThrown,
//                 _                       => TurnoutStateEnum.Unknown // Ignore an Unknown State
//             };
//             Console.WriteLine($"Applying action {action.Id} {turnout.State} to button {actionTurnout.TurnoutID} with state {turnoutState}");
//             // TODO: FIX THIS
//             // actionTurnout.SetTurnoutState(turnoutState);
//             if (action.Cascade) {
//                 Console.WriteLine($"Cascading action {action.Id} to {actionTurnout.TurnoutID}");
//                 //ActionApplyTurnout.ApplyTurnoutActions(action, buttonState, connectionService);
//             }
//         }
//     }
// }