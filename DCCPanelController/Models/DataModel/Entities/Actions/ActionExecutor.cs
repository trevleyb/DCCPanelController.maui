// New file: Actions/ActionExecutor.cs

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public static class ActionExecutor {
    public static async Task ExecuteAsync(IEnumerable<PlannedAction> plan,
        ConnectionService connectionService,
        ActionExecutionContext context) {

        var actions = plan.ToList();
        if (actions.Count == 0) return;

        // 1) LOCAL STATE UPDATE (atomic-ish): no awaits, no I/O
        foreach (var a in actions) {
            using var _ = context.BeginCascade(a.EntityId);
            if (!context.CanCascade(a.EntityId)) continue;
            
            switch (a.Kind) {
                case ActionTargetKind.Button:
                    if (a is { Target: ActionButtonEntity b, NewState: ButtonStateEnum bs }) {
                        b.State = bs;
                    }
                break;

                case ActionTargetKind.Turnout:
                    if (a is { Target: TurnoutEntity t, NewState: TurnoutStateEnum ts }) {
                        t.State = ts;
                    }
                break;
            }
        }

        // 2) EXTERNAL COMMANDS (network) – only for turnouts and only once per entity
        if (connectionService.Client is { } client) {
            foreach (var a in actions.Where(x => x.Kind == ActionTargetKind.Turnout)) {
                if (a is { Target: TurnoutEntity { Turnout: { } turnout } t, NewState: TurnoutStateEnum ts }) {
                    try {
                        await client.SendTurnoutCmdAsync(turnout, ts != TurnoutStateEnum.Closed);
                    } catch (Exception ex) {
                        LogHelper.Logger.LogWarning($"Turnout command failed for {t.Id}: {ex.Message}");
                    }
                }
            }
        }
    }
}