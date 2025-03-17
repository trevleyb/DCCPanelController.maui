using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.ViewModel.Interfaces;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.DynamicProperties;

/// <summary>
///  Popup Page is used for iPad and MacCatalst
/// </summary>
public partial class DynamicPropertyPopup : Popup {
    private DynamicPropertyPageViewModel _viewModel;
    public DynamicPropertyPopup(ITile tile,string? propertyName = null) {
        InitializeComponent();
        _viewModel = new DynamicPropertyPageViewModel(tile, propertyName, PropertyContainer);
        BindingContext = _viewModel;
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        this.Close();
    }
}