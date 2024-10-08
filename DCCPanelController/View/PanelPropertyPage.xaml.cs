using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelPropertyPage : ContentPage {
    
    public PanelPropertyPage(object obj) {
        InitializeComponent();
        BindingContext = new PropertyViewModel(PropertyContainer, obj);
    }
    
    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}