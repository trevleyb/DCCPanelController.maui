using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrack {

    Guid Id { get; set; }
    string Name { get; }
    
    int X { get; set; }
    int Y { get; set; }
    int Layer { get; }
    bool IsSelected { get; set; }

    void RotateLeft();
    void RotateRight();

    IView? TrackViewRef { get; set; }
    Panel? Parent { get; set; }
    TrackConnectionsEnum[] Connections { get; }
    IView TrackView(double gridSize, bool passthrough = false);

    ITrack Clone(Panel parent);
    
    event PropertyChangedEventHandler? PropertyChanged;
}