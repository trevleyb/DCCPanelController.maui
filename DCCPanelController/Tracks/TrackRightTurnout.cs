using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackRightTurnout : TrackPiece, ITrackTurnout {
    
    protected override void Setup() {
        Name = "Right Turnout";
        AddTrackImage(0,   UnknownState, "TurnoutR1", 0);
        AddTrackImage(90,  UnknownState, "TurnoutR1", 90);
        AddTrackImage(180, UnknownState, "TurnoutR1", 180);
        AddTrackImage(270, UnknownState, "TurnoutR1", 270);

        AddTrackImage(0,   "Straight", "TurnoutR2", 0);
        AddTrackImage(90,  "Straight", "TurnoutR2", 90);
        AddTrackImage(180, "Straight", "TurnoutR2", 180);
        AddTrackImage(270, "Straight", "TurnoutR2", 270);

        AddTrackImage(0,   "Diverging", "TurnoutR3", 0);
        AddTrackImage(90,  "Diverging", "TurnoutR3", 90);
        AddTrackImage(180, "Diverging", "TurnoutR3", 180);
        AddTrackImage(270, "Diverging", "TurnoutR3", 270);
    }
}