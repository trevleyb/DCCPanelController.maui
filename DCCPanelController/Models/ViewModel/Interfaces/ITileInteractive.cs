using DCCClients;

namespace DCCPanelController.Models.ViewModel.Interfaces;

public interface ITileInteractive {
    public void Interact(IDccClient? client);
    public void Secondary(IDccClient? client);
}
