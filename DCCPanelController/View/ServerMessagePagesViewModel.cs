using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;

namespace DCCPanelController.View;

public class ServerMessagesViewModel : ConnectionViewModel {
    public ServerMessagesViewModel(ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) { }
}