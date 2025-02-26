using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrack {

    Guid Id { get; set; }
    string Name { get; }
    int Layer { get; }

    int X { get; set; }
    int Y { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    bool IsSelected { get; set; }

    IView? TrackViewRef { get; set; }
    Panel? Parent { get; set; }
    TrackConnectionsEnum[] Connections { get; }

    void RotateLeft();
    void RotateRight();
    IView TrackView(double gridSize, bool passthrough = false);

    ITrack Clone(Panel parent);

    event PropertyChangedEventHandler? PropertyChanged;
}