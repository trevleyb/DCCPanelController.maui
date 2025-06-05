using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITileInteractive {
    public Task Interact(ConnectionService? connectionService);
    public Task Secondary(ConnectionService? connectionService);
}