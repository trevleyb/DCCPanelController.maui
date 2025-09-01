using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.Views.Base;

namespace DCCPanelController.Views;

public partial class ServerMessagesViewModel : ConnectionViewModel {
    public ServerMessagesViewModel(ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) { }
}