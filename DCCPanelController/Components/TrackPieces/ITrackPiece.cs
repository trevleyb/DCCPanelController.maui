using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Components.TrackPieces;

public interface ITrackPiece {
 
    public string Name       { get; set; }
    public string Style      { get; set; }
    public int Rotation      { get; set; }
    public int XCoordinate   { get; set; }
    public int YCoordinate   { get; set; }
    public ImageSource Image { get; }
    
}