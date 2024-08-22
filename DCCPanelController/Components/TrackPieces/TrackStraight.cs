namespace DCCPanelController.Components.TrackPieces;

public class StraightPiece : TrackPiece {
    
    protected override void Setup() {
        _points = 8;
        _rotationMatrix = new int[] { 0, 180, 90, 0 };
        State.SetStates( ("Default", 1), ("Normal", 1));
        AddTrackImage("Straight1");
        AddTrackImage("Straight2");
    }
    
}
