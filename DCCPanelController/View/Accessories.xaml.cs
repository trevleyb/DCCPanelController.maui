using DCCPanelController.Helpers;
using DCCPanelController.View.Helpers;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View;

public partial class AccessoriesPage : ContentPage {
    public AccessoriesPage() {
        BindingContext = this;
        InitializeComponent();
    }

    private async void OnTurnoutsClicked(object? sender, EventArgs e) {
        await Shell.Current.GoToAsync("//Accessories/Turnouts");
    }

    private async void OnRoutesClicked(object? sender, EventArgs e) {
        await Shell.Current.GoToAsync("//Accessories/Routes");
    }

    private async void OnBlocksClicked(object? sender, EventArgs e) {
        await Shell.Current.GoToAsync("//Accessories/Blocks");
    }

    private async void OnLightsClicked(object? sender, EventArgs e) {
        await Shell.Current.GoToAsync("//Accessories/Lights");
    }
}