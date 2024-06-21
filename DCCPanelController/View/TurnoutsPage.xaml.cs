
using DCCPanelController.Services;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {

    public TurnoutsPage() {
        InitializeComponent();
        
        On<iOS>().SetUseSafeArea(false);
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);

        // Resolve the TurnoutStateViewModel dependency
        var service = new Services.TurnoutStateService();
        var viewModel = new TurnoutStateViewModel(service);
        //var viewModel = DependencyService.Get<TurnoutStateViewModel>();
        BindingContext = viewModel;
    }
}