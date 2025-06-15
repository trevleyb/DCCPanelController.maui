using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Services;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class TurnoutActions : ObservableCollection<TurnoutAction>, ICloneable {
    public TurnoutActions() { }

    public TurnoutActions(TurnoutActions buttonActions) {
        foreach (var action in buttonActions) Add(new TurnoutAction(action));
    }

    public void Apply(TurnoutEntity turnout, ConnectionService connectionService) {
        Console.WriteLine($"Applying actions to turnout {turnout.TurnoutID} with state {turnout.State}");

        foreach (var action in turnout.ButtonPanelActions) {
            if (turnout.Parent?.GetButtonEntity(action.Id) is { } actionButton) {
                actionButton.State = turnout.State switch {
                    TurnoutStateEnum.Closed => action.WhenOn,
                    TurnoutStateEnum.Thrown => action.WhenOff,
                    _                       => ButtonStateEnum.Unknown
                };
            }
        }

        foreach (var action in turnout.TurnoutPanelActions) {
            if (turnout.Parent?.GetTurnoutEntity(action.Id) is { } actionTurnout) {
                actionTurnout.State = turnout.State switch {
                    TurnoutStateEnum.Closed => action.WhenClosed,
                    TurnoutStateEnum.Thrown => action.WhenThrown,
                    _                       => TurnoutStateEnum.Unknown
                };
            }
        }
    }
    
    public object Clone() {
        var turnoutActions = new TurnoutActions();
        foreach (var action in this) turnoutActions.Add(new TurnoutAction(action));
        return turnoutActions;
    }

}

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private TurnoutStateEnum _whenClosed = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _whenThrown = TurnoutStateEnum.Unknown;

    public TurnoutAction() { }

    public TurnoutAction(TurnoutAction action) {
        Id = action.Id;
        WhenClosed = action.WhenClosed;
        WhenThrown = action.WhenThrown;
        Cascade = action.Cascade;
    }
}