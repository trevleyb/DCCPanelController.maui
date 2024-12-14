using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackPiece {
    string Name { get; }

    int X { get; set; }
    int Y { get; set; }
    int Layer { get; }
    bool IsSelected { get; set; }

    int TrackRotation { get; set; }
    ImageSource DisplayImage { get; }
    IView GetDisplayItem(double gridSize, bool passthrough = false);
    
    Panel? Parent { get; set; }
    ITrackPiece Clone();
    TrackConnectionsEnum[] Connections { get; }

    event PropertyChangedEventHandler? PropertyChanged;

    void RotateLeft();
    void RotateRight();
}