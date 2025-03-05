using System.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrack {
    Guid UniqueID { get; set; }
    string Name { get; }

    Panel? Parent { get; set; }

    int X { get; set; }
    int Y { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    int Layer { get; }

    bool IsSelected { get; set; }
    bool IsPath { get; set; }
    bool IsOccupied { get; set; }

    IView TrackView(double gridSize, bool? passthrough);
    TrackConnectionsEnum Connection(int direction);

    void InvalidateView();
    void RotateLeft();
    void RotateRight();

    ITrack Clone(Panel parent);

    event PropertyChangedEventHandler? PropertyChanged;
}