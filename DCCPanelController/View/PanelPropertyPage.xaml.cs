using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;

namespace DCCPanelController.View;

public partial class PanelPropertyPage : ContentPage {
    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
    }

    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}