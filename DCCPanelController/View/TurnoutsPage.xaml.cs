using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {
    private readonly ILogger<TurnoutsPage> _logger;
    private readonly TurnoutsViewModel     _viewModel;

    public TurnoutsPage(ILogger<TurnoutsPage> logger, TurnoutsViewModel viewModel) {
        InitializeComponent();
        _logger = logger;
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.SetNavigationReferences(BottomSheet);
        SafeAreaEdges = SafeAreaEdges.None;
        var safeInsets = On<iOS>().SafeAreaInsets();
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        _viewModel.SetToolbarItems();
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;
    }
}