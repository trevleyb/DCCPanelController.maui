using DCCPanelController.Models.DataModel.Entities;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.DynamicProperties;

/// <summary>
///     Dynamic Property Page is used for iOS iPhone sizes and is full screen
/// </summary>
public partial class DynamicPropertyPage : ContentPage {
    public DynamicPropertyPage(List<Entity> entities, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel(entities, propertyName, PropertyContainer);
    }

    public DynamicPropertyPage(Entity entity, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel([entity], propertyName, PropertyContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        if (BindingContext is DynamicPropertyPageViewModel viewModel) {
            viewModel.ApplyChangesToAllEntities();
        }
        Navigation.PopModalAsync(true);
    }
}