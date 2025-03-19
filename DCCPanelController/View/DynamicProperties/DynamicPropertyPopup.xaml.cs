using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.DynamicProperties;

/// <summary>
///  Popup Page is used for iPad and MacCatalst
/// </summary>
public partial class DynamicPropertyPopup : Popup {
    public DynamicPropertyPopup(Entity entity,string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel(entity, propertyName, PropertyContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        this.Close();
    }
}