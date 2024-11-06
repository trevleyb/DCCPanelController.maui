using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.Base;

public interface ITrackPiece {
    public string Name { get; set; }
    public bool IsOccupied { get; set; }
    public int ImageRotation { get; set; }
    public int TrackDirection { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Layer { get; }
    public ImageSource? Image { get; }

    public void NextState();
    public void PrevState();
    public void RotateLeft();
    public void RotateRight();
    public void RotateAbsolute(int degrees);

    public TrackConnectionsEnum[] Connections { get; }
}