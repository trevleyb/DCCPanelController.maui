using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Services;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class ButtonActions : ObservableCollection<ButtonAction>, ICloneable {
    public ButtonActions() { }

    public ButtonActions(ButtonActions buttonActions) {
        foreach (var action in buttonActions) Add(new ButtonAction(action));
    }

    public async void Apply(ButtonEntity button, ConnectionService connectionService, ActionExecutionContext context) {
        Console.WriteLine($"Applying actions to button {button.Id} with state {button.State}");

        foreach (var action in button.ButtonPanelActions) {
            if (button.Parent?.GetButtonEntity(action.Id) is { } actionButton) {
                var newState = button.State switch {
                    ButtonStateEnum.On  => action.WhenOn,
                    ButtonStateEnum.Off => action.WhenOff,
                    _                   => ButtonStateEnum.Unknown
                };
                
                // This is an internal change that should cascade
                actionButton.SetState(newState, StateChangeSource.Internal, context);
            }
        }

        foreach (var action in button.TurnoutPanelActions) {
            if (button.Parent?.GetTurnoutEntity(action.Id) is { } actionTurnout) {
                var newState = button.State switch {
                    ButtonStateEnum.On  => action.WhenClosed,
                    ButtonStateEnum.Off => action.WhenThrown,
                    _                   => TurnoutStateEnum.Unknown
                };
                
                // This is an internal change that should cascade AND be sent to physical layout
                actionTurnout.SetState(newState, StateChangeSource.Internal, context);
                
                // Send command to physical turnout
                if (connectionService.Client is { } client && actionTurnout.Turnout != null) {
                    await client.SendTurnoutCmdAsync(actionTurnout.Turnout, newState != TurnoutStateEnum.Closed);
                }
            }
        }
    }

    public object Clone() {
        var buttonActions = new ButtonActions();
        foreach (var action in this) buttonActions.Add(new ButtonAction(action));
        return buttonActions;
    }
}

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenOff = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenOn = ButtonStateEnum.Unknown;

    public ButtonAction() { }

    public ButtonAction(ButtonAction action) {
        Id = action.Id;
        WhenOn = action.WhenOn;
        WhenOff = action.WhenOff;
    }
}