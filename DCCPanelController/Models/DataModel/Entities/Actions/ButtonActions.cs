using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class ButtonActions : ObservableCollection<ButtonAction>, ICloneable {
    public ButtonActions() { }

    public ButtonActions(ButtonActions buttonActions) {
        foreach (var action in buttonActions) Add(new ButtonAction(action));
    }

    public object Clone() {
        var buttonActions = new ButtonActions();
        foreach (var action in this) buttonActions.Add(new ButtonAction(action));
        return buttonActions;
    }

    public async void Apply(ActionButtonEntity button, ConnectionService connectionService, ActionExecutionContext context) {
        var logger = LogHelper.CreateLogger("ButtonActionsApply");
        try {
            foreach (var action in button.ButtonPanelActions) {
                if (button.Parent?.GetButtonEntity(action.Id) is { } actionButton) {
                    var newState = button.State switch {
                        ButtonStateEnum.On  => action.WhenOn,
                        ButtonStateEnum.Off => action.WhenOff,
                        _                   => ButtonStateEnum.Unknown,
                    };
                    actionButton.SetState(newState, StateChangeSource.Internal, context);
                }
            }

            foreach (var action in button.TurnoutPanelActions) {
                if (button.Parent?.GetTurnoutEntity(action.Id) is { } actionTurnout) {
                    var newState = button.State switch {
                        ButtonStateEnum.On  => action.WhenClosed,
                        ButtonStateEnum.Off => action.WhenThrown,
                        _                   => TurnoutStateEnum.Unknown,
                    };
                    actionTurnout.SetState(newState, StateChangeSource.Internal, context);
                    if (connectionService.Client is { } client && actionTurnout.Turnout != null) {
                        await client.SendTurnoutCmdAsync(actionTurnout.Turnout, newState != TurnoutStateEnum.Closed);
                    }
                }
            }
        } catch (Exception ex) {
            logger.LogError("Error in Async Void function: ButtonActions:Apply =>{Message}", ex.Message);
        }
    }
}

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private string          _id      = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenOff = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenOn  = ButtonStateEnum.Unknown;

    public ButtonAction() { }

    public ButtonAction(ButtonAction action) {
        Id = action.Id;
        WhenOn = action.WhenOn;
        WhenOff = action.WhenOff;
    }
}