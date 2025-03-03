using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using DCCPanelController.Model;

namespace DCCPanelController.View.PropertyPages;

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