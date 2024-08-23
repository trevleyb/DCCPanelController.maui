using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.View;

public partial class TrackPieceTestPage : ContentPage {
    
    private readonly TrackPieceTestViewModel _viewModel;
    
    public TrackPieceTestPage() {
        InitializeComponent();
        try {
            _viewModel = new TrackPieceTestViewModel();
            BindingContext = _viewModel;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}