using DCCPanelController.Clients;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {
    private readonly TurnoutsViewModel _viewModel;

    public TurnoutsPage(TurnoutsViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.Navigation = Navigation;
        On<iOS>().SetUseSafeArea(false);
        var safeInsets = On<iOS>().SafeAreaInsets();
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        _viewModel.IsSupported = _viewModel.Profile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Turnouts) ?? false;
        _viewModel.CanAddTurnout = _viewModel.Profile?.Settings?.ClientSettings?.SupportsManualEntries == true && _viewModel.IsSupported;
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;
    }
}