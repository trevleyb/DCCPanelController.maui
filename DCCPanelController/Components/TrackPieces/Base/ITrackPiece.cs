namespace DCCPanelController.Components.TrackPieces.Base;

public interface ITrackPiece {
 
    public string Name        { get; set; }
    public string Style       { get; set; }
    public int Rotation       { get; set; }
    public int X              { get; set; }
    public int Y              { get; set; }
    public ImageSource? Image { get; }
    
    public TrackState State { get; }
    public void NextState();
    public void PrevState();
    public void RotateLeft();
    public void RotateRight();
    public void RotateAbsolute(int degrees);
    public void RotateRelative(int degrees);
}