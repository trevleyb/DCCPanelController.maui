using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class LightsPage : ContentPage {
    private readonly ILogger<LightsPage> _logger;

    public LightsPage(ILogger<LightsPage> logger, LightsViewModel viewModel) {
        _logger = logger;
        BindingContext = viewModel;
        viewModel.SetNavigationReferences(BottomSheet);
        InitializeComponent();

        SafeAreaEdges = SafeAreaEdges.None;
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
    
    protected override void OnAppearing() {
        base.OnAppearing();
        if (BindingContext is LightsViewModel viewModel) viewModel.SetToolbarItems();
    }
}