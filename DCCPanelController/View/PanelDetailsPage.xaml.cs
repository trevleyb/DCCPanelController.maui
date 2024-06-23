using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;

namespace DCCPanelController.View;

public partial class PanelDetailsPage : ContentPage {
    public PanelDetailsPage(Panel panel) {
        InitializeComponent();
        // var service = App.ServiceProvider?.GetService<PanelDetailsService>();
        //_viewModel = new PanelDetailsViewModel(service);
        BindingContext = panel;
    }
}