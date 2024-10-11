using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;

namespace DCCPanelController.View.PropertPages;

public partial class PanelPropertyPage : ContentView {
    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
        BindingContext = panel;
    }
}