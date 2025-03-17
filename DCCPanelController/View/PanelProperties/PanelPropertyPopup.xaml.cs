using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.PanelProperties;

public partial class PanelPropertyPopup : Popup {
    public PanelPropertyPopup(Panel panel) {
        InitializeComponent();
        var propertyDetails = new PanelPropertyBase(panel);
        Properties.Children.Add(propertyDetails);
        BindingContext = propertyDetails.BindingContext;
    }
    
    private void ClosePropertyPage(object? sender, EventArgs? e) {
        this.Close();
    }

}