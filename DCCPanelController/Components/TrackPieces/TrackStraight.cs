namespace DCCPanelController.Components.TrackPieces;

public class StraightPiece : TrackPiece {
    
    protected override void Setup() {
        _points = 8;
        _rotationMatrix = new int[] { 0, 180, 90, 0 }; 
        
        AddTrackImage("Straight1");
        AddTrackImage("Straight2");
    }
    
}
