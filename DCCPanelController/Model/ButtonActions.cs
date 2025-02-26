using System.Collections.ObjectModel;

namespace DCCPanelController.Model;

public class ButtonActions : ObservableCollection<ButtonAction> {

    public void ApplyButtonActionsToPanel<TEnum>(Panel panel, TEnum state) where TEnum : Enum {
        ApplyButtonActionsToPanel(panel, state, this);
    }

    public void ApplyButtonActionsToPanel<TEnum>(Panel panel, TEnum state, IEnumerable<ButtonAction> actions) where TEnum : Enum {
        Console.WriteLine($"Applying Button Actions to Panel: {panel.Name}");
        var applied = new List<string>();
        foreach (var action in actions) {
            Console.WriteLine($"Applying {typeof(TEnum).Name.ToUpper()} Button Action: {action.Id} to Panel: {panel.Name} for button: {action.Id}");
            ProcessButtonAction(panel, action, state, applied);
        }
    }

    private void ProcessButtonAction<TEnum>(Panel panel, ButtonAction action, TEnum state, List<string> applied) where TEnum : Enum {
        // Get the button that this action relates to
        // ------------------------------------------------------------
        var button = panel.GetButton(action.Id);
        if (button is null) return;

        // Based on the state provided (the state of the controlling button) set the state of the 
        // related button. 
        // ---------------------------------------------------------------------------------------
        switch (state) {
        case ButtonStateEnum buttonStateEnum:
            _ = buttonStateEnum switch {
                ButtonStateEnum.Active   => button.SetButtonState(action.WhenActiveOn),
                ButtonStateEnum.Inactive => button.SetButtonState(action.WhenInactiveOff),
                _                        => false // Ignore an Unknown State
            };

            break;
        case TurnoutStateEnum turnoutStateEnum:
            _ = turnoutStateEnum switch {
                TurnoutStateEnum.Closed => button.SetButtonState(action.WhenActiveOn),
                TurnoutStateEnum.Thrown => button.SetButtonState(action.WhenInactiveOff),
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
            button.ExecButtonState();
        }
    }
}