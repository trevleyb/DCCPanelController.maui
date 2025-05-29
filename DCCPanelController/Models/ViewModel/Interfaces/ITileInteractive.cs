using DCCPanelController.Services;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITileInteractive {
    public void Interact(ConnectionService? connectionService);
    public void Secondary(ConnectionService? connectionService);
}