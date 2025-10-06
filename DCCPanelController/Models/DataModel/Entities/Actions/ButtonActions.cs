using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
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

    // replace current Apply with:
    public async Task ApplyAsync(ActionButtonEntity button, ConnectionService connectionService, ActionExecutionContext context) {
        try {
            var plan = ActionPlanner.PlanForButtonChange(button);
            await ActionExecutor.ExecuteAsync(plan, connectionService, context);
        } catch (Exception ex) {
            var logger = LogHelper.CreateLogger("ButtonActionsApply");
            logger.LogError(ex, "ButtonActions.ApplyAsync failed");
            throw;
        }
    }
}

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private string          _actionID = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenOff  = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenOn   = ButtonStateEnum.Unknown;

    public ButtonAction() { }

    public ButtonAction(ButtonAction action) {
        ActionID = action.ActionID;
        WhenOn = action.WhenOn;
        WhenOff = action.WhenOff;
    }
}