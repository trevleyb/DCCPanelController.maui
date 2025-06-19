using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITileInteractive {
    public Task<bool> Interact(ConnectionService? connectionService);
    public Task<bool> Secondary(ConnectionService? connectionService);
}