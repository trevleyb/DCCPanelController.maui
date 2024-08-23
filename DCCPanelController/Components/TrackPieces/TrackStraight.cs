namespace DCCPanelController.Components.TrackPieces;

public class StraightPiece : Base.TrackPiece {
    
    protected override void Setup() {
        SetRotationMatrix([0, 90, 90, 0, 0, 90, 90, 0]);
        State.SetStates(("Normal", 1));
        AddTrackImage("Straight1");
        AddTrackImage("Straight2");
    }
    
}
