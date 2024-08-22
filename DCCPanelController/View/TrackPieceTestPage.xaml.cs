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
    private readonly ObservableCollection<Line> _lines = [];
    private double _factor = 1;
    private bool _drawCompass = false;
    private string style = "";
    
    public TrackPieceTestPage() {
        InitializeComponent();
        try {
            _viewModel = new TrackPieceTestViewModel();
            BindingContext = _viewModel;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private void Mainline_Button_OnClicked(object? sender, EventArgs e) {
        style = "Mainline";
    }

    private void Branchline_Button_OnClicked(object? sender, EventArgs e) {
        style = "Branchline";
    }

    private void MainlineHidden_Button_OnClicked(object? sender, EventArgs e) {
        style = "MainlineDashed";
    }

    private void BranchlineHidden_Button_OnClicked(object? sender, EventArgs e) {
        style = "BranchlineDashed";
    }

    private void CompassOn_Button_OnClicked(object? sender, EventArgs e) {
        _drawCompass = !_drawCompass;
    }

    private void Slider_OnValueChanged(object? sender, ValueChangedEventArgs e) {
    }

    private void Rotate_Button_OnClicked(object? sender, EventArgs e) {
        _viewModel.Rotation += 45;
        if (_viewModel.Rotation >= 360) _viewModel.Rotation = 0;
    }

    private void Export_Button_OnClicked(object? sender, EventArgs e) {
        var json = TrackStyles.Export();
        TrackStyles.Import(json);
    }
    
    private void Button_OnClicked(object? sender, EventArgs e) {
        _factor = _factor < 1.5 ? 1.5 : 1.0;
    }

    private void BindableObject_OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (_viewModel == null || _viewModel.Tracks == null) return;
        foreach (var track in _viewModel.Tracks) {
            if (track.Track is { SupportsLabel: true } piece) piece.SetLabel(_viewModel.Label ?? "");
        }
    }
}