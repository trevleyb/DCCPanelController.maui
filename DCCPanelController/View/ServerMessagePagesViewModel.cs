using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;

namespace DCCPanelController.View;

public partial class ServerMessagesViewModel(ProfileService profileService, ConnectionService connectionService)
    : ConnectionViewModel(profileService, connectionService) {
    
    [ObservableProperty] private bool _isWide;
    [ObservableProperty] private bool _isNarrow;
    
}
    
    