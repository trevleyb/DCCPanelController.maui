using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITile {
    Entity Entity { get; }
    void CreateTile();
    bool IsSelected { get; set; }
}