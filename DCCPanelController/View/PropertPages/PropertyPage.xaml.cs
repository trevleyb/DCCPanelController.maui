using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCPanelController.View.PropertPages;

public partial class PropertyPage : ContentPage {
    public PropertyPage(object model) {
        var result = PropertyPageFactory.CreatePropertyPage(model);
        if (result.IsSuccess) {
            InitializeComponent();
            PropertyContainer.Children.Clear();
            PropertyContainer.Children.Add(result.Value);
        } else {
            Navigation.PopModalAsync();
        }
    }

    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}