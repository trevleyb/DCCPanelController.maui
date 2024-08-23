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
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            BindingContext = _viewModel;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
    
    private void AddTrackPiece(TrackPiece track, Grid grid) {
        var image = new Image();
        image.Source = track?.Svg?.Image;
        //image.HeightRequest = _viewModel.ComponentHeight;
        //image.WidthRequest = _viewModel.ComponentWidth;
        //image.Rotation = _viewModel.Rotation;
        image.ZIndex = 0;
        grid.Children.Add(image);
        grid.SetColumn(image,track.Col);
        grid.SetRow(image, track.Row);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Property {e.PropertyName} changed from {sender?.ToString()}");
    }
}