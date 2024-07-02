using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage {
    public PanelsPage() {
        InitializeComponent();
        var service = App.ServiceProvider?.GetService<SettingsService>();
        var viewModel = new PanelsViewModel(service!, this);
        BindingContext = viewModel;
    }

    private async void ToolBarAddNewItemClicked(object? sender, EventArgs e) {
        try {
            await Navigation.PushAsync(new PanelDetailsPage(new Panel()));
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to navigate to Panel Details: {ex.Message}");
        }
    }
}