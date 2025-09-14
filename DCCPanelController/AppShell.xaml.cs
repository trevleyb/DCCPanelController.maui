using DCCPanelController.Services;

namespace DCCPanelController;

public partial class AppShell : Shell {
    public AppShell() => InitializeComponent();

    public AppStateService AppState => AppStateService.Instance;
}