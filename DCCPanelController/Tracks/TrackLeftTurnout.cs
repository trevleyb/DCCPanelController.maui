using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackLeftTurnout : TrackPiece, ITrackTurnout {
    protected override void Setup() {
        Name = "Left Turnout";
        AddTrackImage(0,   UnknownState, "TurnoutL1", 0);
        AddTrackImage(90,  UnknownState, "TurnoutL1", 90);
        AddTrackImage(180, UnknownState, "TurnoutL1", 180);
        AddTrackImage(270, UnknownState, "TurnoutL1", 270);

        AddTrackImage(0,   "Straight", "TurnoutL2", 0);
        AddTrackImage(90,  "Straight", "TurnoutL2", 90);
        AddTrackImage(180, "Straight", "TurnoutL2", 180);
        AddTrackImage(270, "Straight", "TurnoutL2", 270);

        AddTrackImage(0,   "Diverging", "TurnoutL3", 0);
        AddTrackImage(90,  "Diverging", "TurnoutL3", 90);
        AddTrackImage(180, "Diverging", "TurnoutL3", 180);
        AddTrackImage(270, "Diverging", "TurnoutL3", 270);
    }
}