using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PropertyPage : ContentPage {
    public PropertyPage(string propertyName, object obj) {
        InitializeComponent();
        BindingContext = new PropertyPageViewModel(propertyName, TableViewContainer, obj);
    }

    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}