
using DCCPanelController.Services;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class RoutesPage : ContentPage {

    public RoutesPage() {
        InitializeComponent();
        
        On<iOS>().SetUseSafeArea(false);
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);

        // Resolve the TurnoutStateViewModel dependency
        //var service = App.ServiceProvider?.GetService<RoutesService>();
        var viewModel = App.ServiceProvider?.GetService<RoutesViewModel>();
        BindingContext = viewModel;
    }
}