using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class SensorsPage : ContentPage {
    private readonly ILogger<SensorsPage> _logger;

    public SensorsPage(ILogger<SensorsPage> logger, SensorsViewModel viewModel) {
        InitializeComponent();
        _logger = logger;
        viewModel.SetNavigationReferences(BottomSheet);
        BindingContext = viewModel;

        SafeAreaEdges = SafeAreaEdges.None;
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
    
    protected override void OnAppearing() {
        base.OnAppearing();
        if (BindingContext is SensorsViewModel viewModel) viewModel.SetToolbarItems();
    }

}