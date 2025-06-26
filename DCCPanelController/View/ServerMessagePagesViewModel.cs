using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.View.Base;

namespace DCCPanelController.View;

public partial class ServerMessagesViewModel : ConnectionViewModel {
    public ServerMessagesViewModel(ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) { }
}