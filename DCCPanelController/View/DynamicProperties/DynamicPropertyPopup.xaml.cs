using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel.Entities;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.DynamicProperties;

/// <summary>
///     Popup Page is used for iPad and MacCatalst
/// </summary>
public partial class DynamicPropertyPopup : Popup {
    public DynamicPropertyPopup(Entity entity, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel([entity], propertyName, PropertyContainer);
    }

    public DynamicPropertyPopup(List<Entity> entities, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel(entities, propertyName, PropertyContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        if (BindingContext is DynamicPropertyPageViewModel viewModel) {
            viewModel.ApplyChangesToAllEntities();
        }
        Close();
    }
}