using System.ComponentModel;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {
    public TurnoutsPage(TurnoutsViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += (sender, args) => {
            Console.WriteLine($"PROPERTY CHANGED: {args.PropertyName}");
            Console.WriteLine($"CONNECTION ICON: {viewModel.ConnectionIcon}");
        };
        
        On<iOS>().SetUseSafeArea(false);
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
}