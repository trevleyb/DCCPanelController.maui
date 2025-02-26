using System.Collections.ObjectModel;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.View;

namespace DCCPanelController.Model;

public class TurnoutActions : ObservableCollection<TurnoutAction> {

    public void ApplyTurnoutActionsToPanel<TEnum>(Panel panel, TEnum state) where TEnum : Enum {
        Console.WriteLine($"Applying Turnout Actions to Panel: {panel.Name}");
        var applied = new List<string>();
        foreach (var action in this) {
            Console.WriteLine($"Applying {typeof(TEnum).Name.ToUpper()} Turnout Action: {action.Id} to Panel: {panel.Name} for turnout: {action.Id}");
            ProcessTurnoutAction(panel, action, state, applied);
        }
    }

    private static void ProcessTurnoutAction<TEnum>(Panel panel, TurnoutAction action, TEnum state, List<string> applied) where TEnum : Enum {
        // Get the turnout that this action relates to
        // ------------------------------------------------------------
        var turnout = panel.GetTurnout(action.Id);
        if (turnout is null) return;

        // Based on the state provided (the state of the controlling button) set the state of the 
        // related button. 
        // ---------------------------------------------------------------------------------------
        switch (state) {
        case ButtonStateEnum buttonStateEnum:
            _ = buttonStateEnum switch {
                ButtonStateEnum.Active   => turnout.SetTurnoutState(action.WhenClosedStraight),
                ButtonStateEnum.Inactive => turnout.SetTurnoutState(action.WhenThrownDiverging),
                _                        => false // Ignore an Unknown State
            };
            break;
        case TurnoutStateEnum turnoutStateEnum:
            _ = turnoutStateEnum switch {
                TurnoutStateEnum.Closed => turnout.SetTurnoutState(action.WhenClosedStraight),
                TurnoutStateEnum.Thrown => turnout.SetTurnoutState(action.WhenThrownDiverging),
                _                       => false // Ignore an Unknown State
            };
            break;
        }
        
        // If we have marked this Action as a Cascading Action, and if we have not processed
        // this button or turnout before, then cascade and execute the cascaded actions.
        // ----------------------------------------------------------------------------------
        if (action.Cascade && applied.Contains(action.Id) == false) {
            Console.WriteLine($"Cascading {action.Id} to {action.Cascade}");
            applied.Add(action.Id);
            turnout.ExecTurnoutState();
        }
    }
}