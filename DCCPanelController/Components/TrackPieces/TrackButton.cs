using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackButton : TrackPiece {
    protected override void Setup() {
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        
        AddTrackImage(0,"Corner", "Button_Corner", 0);
        AddTrackImage(0,"Corner", "Button_Corner", 90);
        AddTrackImage(0,"Corner", "Button_Corner", 180);
        AddTrackImage(0,"Corner", "Button_Corner", 270);
    }
}