using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class ServerMessagesViewModel : Base.ConnectionViewModel {
    public ServerMessagesViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        ArgumentNullException.ThrowIfNull(Profile);
    }
}