using System.ComponentModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class TurnoutsPage : ContentPage {
    private readonly TurnoutsViewModel _viewModel;
    public TurnoutsPage(TurnoutsViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        On<iOS>().SetUseSafeArea(false);
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);
    }
    
    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e?.PropertyName) {
        case nameof(TurnoutsViewModel.IsConnected):
            ConnectButton.IconImageSource = _viewModel.IsConnected ? "wifi.png" : "wifi_off.png";
            break;
        }
    }

}