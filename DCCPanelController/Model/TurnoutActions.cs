using System.Collections.ObjectModel;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.Model;

public class TurnoutActions : ObservableCollection<TurnoutAction> {
    public void ApplyTurnoutActionsToPanel(Panel panel, ButtonStateEnum state) {
        foreach (var turnoutAction in this) {
            var track = panel.Tracks.FirstOrDefault(x => x.Id.Equals(turnoutAction.Id));
            if (track is ITrackTurnout turnout) {
                switch (state) {
                case ButtonStateEnum.Active:
                    turnout.ExecTurnoutState(turnoutAction.WhenClosedOrActive);
                    break;
                case ButtonStateEnum.InActive:
                    turnout.ExecTurnoutState(turnoutAction.WhenThrownOrInActive);
                    break;
                }
            }
        }
    }

    public void ApplyTurnoutActionsToPanel(Panel panel, TurnoutStateEnum state) {
        foreach (var turnoutAction in this) {
            var track = panel.Tracks.FirstOrDefault(x => x.Id.Equals(turnoutAction.Id));
            if (track is ITrackTurnout turnout) {
                switch (state) {
                case TurnoutStateEnum.Closed:
                    turnout.ExecTurnoutState(turnoutAction.WhenClosedOrActive);
                    break;
                case TurnoutStateEnum.Thrown:
                    turnout.ExecTurnoutState(turnoutAction.WhenThrownOrInActive);
                    break;
                }
            }
        }
    }
}