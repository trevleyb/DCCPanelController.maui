using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.Interfaces;

public interface ITrackPiece {
 
    public string Name          { get; set; }
    public string Style         { get; set; }
    public bool IsOccupied      { get; set; }
    public int ImageRotation    { get; set; }
    public int TrackDirection   { get; set; }
    public int X                { get; set; }
    public int Y                { get; set; }
    public ImageSource? Image   { get; }
    public TrackState State     { get; }

    public void NextState();
    public void PrevState();
    public void RotateLeft();
    public void RotateRight();
    public void RotateAbsolute(int degrees);
    
    public SvgCompass Connections { get; }
}