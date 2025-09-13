using DCCPanelController.Services;

namespace DCCPanelController;

public partial class AppShell : Shell {
    
    public AppStateService AppState => AppStateService.Instance;
    
    public AppShell() {
        InitializeComponent();
    }
}