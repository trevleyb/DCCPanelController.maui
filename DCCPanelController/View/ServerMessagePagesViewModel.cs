using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.View.Base;

namespace DCCPanelController.View;

public partial class ServerMessagesViewModel : ConnectionViewModel {
    public ServerMessagesViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        ArgumentNullException.ThrowIfNull(Profile);
    }
}