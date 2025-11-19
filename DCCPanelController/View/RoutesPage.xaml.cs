using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class RoutesPage : ContentPage {
    private readonly ILogger<RoutesPage> _logger;

    public RoutesPage(ILogger<RoutesPage> logger, RoutesViewModel viewModel) {
        _logger = logger;
        BindingContext = viewModel;
        InitializeComponent();

        SafeAreaEdges = SafeAreaEdges.None;
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
}