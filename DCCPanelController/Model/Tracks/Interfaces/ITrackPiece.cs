using System.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackPiece {
    Guid Id { get; }
    string Name { get; }
    int X { get; set; }
    int Y { get; set; }
    int Layer { get; }
    bool IsSelected { get; set; }

    IView? TrackViewRef { get; set; }
    IView TrackView(double gridSize, bool passthrough = false);

    //ImageSource DisplayImage { get; }
    //IView GetDisplayItem(double gridSize, bool passthrough = false);
    //IView? ViewReference { get; set; }

    Panel? Parent { get; set; }
    TrackConnectionsEnum[] Connections { get; }

    event PropertyChangedEventHandler? PropertyChanged;

    void RotateLeft();
    void RotateRight();
}