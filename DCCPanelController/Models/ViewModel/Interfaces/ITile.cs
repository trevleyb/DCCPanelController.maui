using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITile {
    Entity Entity { get; }
    bool IsSelected { get; set; }
    void ForceRedraw();
    public event EventHandler<TileChangedEventArgs>? TileChanged;
}