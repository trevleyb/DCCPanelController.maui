using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackPiece {
    string Name { get; }

    int ImageRotation { get; set; }
    int TrackRotation { get; set; }
    int X { get; set; }
    int Y { get; set; }
    int Layer { get; }
    double GridSize { get; }
    bool IsSelected { get; set; }

    IView GetDisplayItem(double gridSize, bool passthrough = false);
    
    Panel? Parent { get; set; }
    ITrackPiece Clone();
    TrackConnectionsEnum[] Connections { get; }

    event PropertyChangedEventHandler? PropertyChanged;

    void RotateLeft();
    void RotateRight();
}