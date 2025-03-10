using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITile {
    Entity Entity { get; }

    void RotateLeft();
    void RotateRight();
    
    void CreateTile();
    bool IsSelected { get; set; }
}