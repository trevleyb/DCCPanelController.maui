using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.TrackPieces.Interfaces;

public interface ITrackPiece {
    
    event PropertyChangedEventHandler? PropertyChanged;
    
    string Name { get; set; }
    
    int TrackRotation { get; set; }
    int TrackDirection { get; set; }
    int X { get; set; }
    int Y { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    int Layer { get; }

    void RotateLeft();
    void RotateRight();
    
    ImageSource? Image { get; }

    TrackConnectionsEnum[] Connections { get; }
}