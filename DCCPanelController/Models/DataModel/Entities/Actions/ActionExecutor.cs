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
            
            Debug.WriteLine($"Executing Action: {a.Kind} {a.EntityId} {a.NewState}");
            
            if (!context.CanCascade(a.EntityId)) {
                Console.WriteLine($"Skipping Action: {a.Kind} {a.EntityId} {a.NewState} due to no-cascade");
                continue;
            }

            using var _ = context.BeginCascade(a.EntityId);
            switch (a.Kind) {
            case ActionTargetKind.Button:
                Console.WriteLine($"Setting Button Action: {a.Kind} {a.EntityId} {a.NewState}");
                if (a is { Target: ActionButtonEntity b, NewState: ButtonStateEnum bs }) {
                    b.State = bs;
                }

                break;

            case ActionTargetKind.Turnout:
                Console.WriteLine($"Setting Turnout Action: {a.Kind} {a.EntityId} {a.NewState}");
                if (a is { Target: Turnout t, NewState: TurnoutStateEnum ts }) {
                    t.State = ts;
                }

                break;
            }
        }

        // 2) EXTERNAL COMMANDS (network) – only for turnouts and only once per entity
        if (connectionService.Client is { } client) {
            foreach (var a in actions.Where(x => x.Kind == ActionTargetKind.Turnout)) {
                Console.WriteLine($"Checking Turnout Command: {a.Kind} {a.EntityId} {a.NewState}");
                if (a is { Target: Turnout { } t, NewState: TurnoutStateEnum ts }) {
                    try {
                        Console.WriteLine($"Sending Turnout Command: {a.Kind} {a.EntityId} {a.NewState}");
                        await client.SendTurnoutCmdAsync(t, ts != TurnoutStateEnum.Closed);
                    } catch (Exception ex) {
                        LogHelper.Logger.LogWarning($"Turnout command failed for {t.Id}: {ex.Message}");
                    }
                }
            }
        }
    }
}