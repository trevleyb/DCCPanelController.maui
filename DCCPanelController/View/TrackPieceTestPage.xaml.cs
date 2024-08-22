using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Components.TrackImages;
using DCCPanelController.Components.Tracks;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.View;

public partial class TrackPieceTestPage : ContentPage {
    
    private readonly TrackPieceTestViewModel _viewModel;
    
    public TrackPieceTestPage() {
        InitializeComponent();
        this.PropertyChanged += OnPropertyChanged;
        try {
            _viewModel = new TrackPieceTestViewModel();
            _viewModel.PropertyChanged += OnPropertyChanged;
            BindingContext = _viewModel;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Property {e.PropertyName} changed");
    }
}