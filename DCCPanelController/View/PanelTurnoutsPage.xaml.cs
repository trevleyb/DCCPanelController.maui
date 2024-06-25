using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;

namespace DCCPanelController.View;

public partial class PanelTurnoutsPage : ContentPage {
    public PanelTurnoutsPage(Panel panel) {
        InitializeComponent();
        BindingContext = panel;
    }
}