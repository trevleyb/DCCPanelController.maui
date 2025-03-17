using DCCPanelController.Models.ViewModel.Interfaces;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.DynamicProperties;

/// <summary>
/// Dynamic Property Page is used for iOS iPhone sizes and is full screen
/// </summary>
public partial class DynamicPropertyPage : ContentPage {
    private DynamicPropertyPageViewModel _viewModel;

    public DynamicPropertyPage(ITile tile, string? propertyName = null) {
        InitializeComponent();
        _viewModel = new DynamicPropertyPageViewModel(tile, propertyName, PropertyContainer);
        BindingContext = _viewModel;
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        Navigation.PopModalAsync(true);
    }
}