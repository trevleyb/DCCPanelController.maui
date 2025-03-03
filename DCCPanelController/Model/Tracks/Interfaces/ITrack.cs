using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrack {
    Guid UniqueID { get; set; }
    string Name { get; }
    int Layer { get; }

    int X { get; set; }
    int Y { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    bool IsSelected { get; set; }
    bool IsPath { get; set; }

    IView? TrackViewRef { get; set; }
    Panel? Parent { get; set; }

    TrackConnectionsEnum Connection(int direction);
    
    void RotateLeft();
    void RotateRight();
    IView TrackView(double gridSize, bool passthrough = false);

    ITrack Clone(Panel parent);

    event PropertyChangedEventHandler? PropertyChanged;
}