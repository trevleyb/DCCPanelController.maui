using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class DynamicPropertyPage : ContentPage {
    
    public DynamicPropertyPage(string propertyName, object obj) {
        InitializeComponent();
        BindingContext = new PropertyViewModel(propertyName, PropertyContainer, obj);
    }
    
    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}