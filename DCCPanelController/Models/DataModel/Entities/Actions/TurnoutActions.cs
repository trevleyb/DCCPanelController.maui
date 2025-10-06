using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
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

    public async Task ApplyAsync(TurnoutEntity turnout, ConnectionService connectionService, ActionExecutionContext context) {
        try {
            var plan = ActionPlanner.PlanForTurnoutChange(turnout);
            await ActionExecutor.ExecuteAsync(plan, connectionService, context);
        } catch (Exception ex) {
            var logger = LogHelper.CreateLogger("TurnoutActionsApply");
            logger.LogError(ex, "TurnoutActions.ApplyAsync failed");
            throw;
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