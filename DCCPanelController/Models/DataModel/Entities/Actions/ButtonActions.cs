using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Services;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class ButtonActions : ObservableCollection<ButtonAction>, ICloneable {

    public ButtonActions() { }

    public ButtonActions(ButtonActions buttonActions) {
        foreach (var action in buttonActions) Add(new ButtonAction(action));
    }

    public void Apply(ButtonEntity button,  ConnectionService connectionService) {
        Console.WriteLine($"Applying actions to button {button.Id} with state {button.State}");

        foreach (var action in button.ButtonPanelActions) {
            if (button.Parent?.GetButtonEntity(action.Id) is { } actionButton) {
                actionButton.State = button.State switch {
                    ButtonStateEnum.On  => action.WhenOn,
                    ButtonStateEnum.Off => action.WhenOff,
                    _                   => ButtonStateEnum.Unknown
                };
            }
        }

        foreach (var action in button.TurnoutPanelActions) {
            if (button.Parent?.GetTurnoutEntity(action.Id) is { } actionTurnout) {
                actionTurnout.State = button.State switch {
                    ButtonStateEnum.On  => action.WhenClosed,
                    ButtonStateEnum.Off => action.WhenThrown,
                    _                   => TurnoutStateEnum.Unknown
                };
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
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenOff = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenOn = ButtonStateEnum.Unknown;

    public ButtonAction() { }

    public ButtonAction(ButtonAction action) {
        Id = action.Id;
        WhenOn = action.WhenOn;
        WhenOff = action.WhenOff;
        Cascade = action.Cascade;
    }
}