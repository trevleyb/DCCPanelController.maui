using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITile {
    Entity Entity { get; }
    bool IsSelected { get; set; }
    void ForceRedraw();
}