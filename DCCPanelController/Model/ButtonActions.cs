using System.Collections.ObjectModel;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.Model;

public class ButtonActions : ObservableCollection<ButtonAction> {

    public void ApplyButtonActionsToPanel(Panel panel, TurnoutStateEnum state) {
        foreach (var buttonAction in this) {
            var track = panel.Tracks.FirstOrDefault(x => x.Id.Equals(buttonAction.Id));
            if (track is ITrackButton button) {
                switch (state) {
                case TurnoutStateEnum.Closed:
                    button.SetButtonState(buttonAction.WhenActiveOrClosed);
                    break;
                case TurnoutStateEnum.Thrown:
                    button.SetButtonState(buttonAction.WhenInactiveOrThrown);
                    break;
                }
            }
        }
    }

    public void ApplyButtonActionsToPanel(Panel panel, ButtonStateEnum state) {
        foreach (var buttonAction in this) {
            var track = panel.Tracks.FirstOrDefault(x => x.Id.Equals(buttonAction.Id));
            if (track is ITrackButton button) {
                switch (state) {
                case ButtonStateEnum.Active:
                    button.SetButtonState(buttonAction.WhenActiveOrClosed);
                    break;
                case ButtonStateEnum.Inactive:
                    button.SetButtonState(buttonAction.WhenInactiveOrThrown);
                    break;
                }
            }
        }
    }
}