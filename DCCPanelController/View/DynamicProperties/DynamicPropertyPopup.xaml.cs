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
    public DynamicPropertyPopup(ITile tile,string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageTableViewModel(tile, propertyName, PropertyContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        this.Close();
    }
}