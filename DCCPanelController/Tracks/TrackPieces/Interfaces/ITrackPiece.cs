using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.TrackPieces.Interfaces;

public interface ITrackPiece {
    string Name { get; }

    int ImageRotation { get; set; }
    int TrackRotation { get; set; }
    int X { get; set; }
    int Y { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    int Layer { get; }
    bool IsSelected { get; set; }
    
    ImageSource? Image { get; }

    ITrackPiece Clone();
    TrackConnectionsEnum[] Connections { get; }

    event PropertyChangedEventHandler? PropertyChanged;

    void RotateLeft();
    void RotateRight();
}