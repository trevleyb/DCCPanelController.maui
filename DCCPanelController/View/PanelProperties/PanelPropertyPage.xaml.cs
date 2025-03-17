using DCCPanelController.Models.DataModel;
#if IOS
using UIKit;
using UIModalPresentationStyle = UIKit.UIModalPresentationStyle;
#endif

namespace DCCPanelController.View.PanelProperties;

public partial class PanelPropertyPage : ContentPage {
    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
        var propertyDetails = new PanelPropertyBase(panel);
        Properties.Children.Add(propertyDetails);
        BindingContext = propertyDetails.BindingContext;
   }

    private void ClosePropertyPage(object? sender, EventArgs? e) {
        Navigation.PopModalAsync(true);
        //CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}