using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class SensorsPage : ContentPage {
    private readonly ILogger<SensorsPage> _logger;

    public SensorsPage(ILogger<SensorsPage> logger, SensorsViewModel viewModel) {
        _logger = logger;
        InitializeComponent();
        BindingContext = viewModel;

        On<iOS>().SetUseSafeArea(false);
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
}