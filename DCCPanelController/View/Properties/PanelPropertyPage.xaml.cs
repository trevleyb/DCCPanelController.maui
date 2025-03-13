using System.Globalization;
using DCCPanelController.Models.DataModel;
using Entry = Microsoft.Maui.Controls.Entry;
#if IOS
using UIKit;
using UIModalPresentationStyle = UIKit.UIModalPresentationStyle;
#endif

namespace DCCPanelController.View.PropertyPages;

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