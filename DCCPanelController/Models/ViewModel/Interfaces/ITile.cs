using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITile {
    Guid Guid { get; }
    Entity Entity { get; }

    public double TileWidth { get; }
    public double TileHeight { get; }
    public double GridSize { get; set; }

    bool IsSelected { get; set; }
    void ForceRedraw();
    public event EventHandler<TileChangedEventArgs>? TileChanged;
}