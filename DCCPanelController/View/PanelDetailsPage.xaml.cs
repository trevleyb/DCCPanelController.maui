using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelDetailsPage : ContentPage {
    private readonly PanelsDetailsViewModel _viewModel;
    public PanelDetailsPage(Panel panel) {
        InitializeComponent();
        _viewModel = new PanelsDetailsViewModel(panel, this);
        BindingContext = _viewModel;
    }
    
}

