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

public partial class ElementTestPage : ContentPage {
    
    private readonly ElementTestViewModel _viewModel;
    private readonly ObservableCollection<Line> _lines = [];
    private double _factor = 1;
    private bool _drawCompass = false;
    private string style = "";
    
    public ElementTestPage() {
        InitializeComponent();
        try {
            _viewModel = new ElementTestViewModel();
            BindingContext = _viewModel;
            BuildTrackPieceGrid();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private void BuildTrackPieceGrid() {
        var grid = TestPiecesGrid;
        grid.Children.Clear();

        var gridSize = _viewModel.Scale * 16;
        for (var i = 0; i < 5; i++) TestPiecesGrid.RowDefinitions[i].Height = new GridLength(gridSize); 
        for (var j = 0; j < 6; j++) TestPiecesGrid.ColumnDefinitions[j].Width = new GridLength(gridSize);
        _viewModel.ComponentWidth = (int)(gridSize * _factor);
        _viewModel.ComponentHeight = (int)(gridSize * _factor);

        foreach (var track in _viewModel.Tracks) {
            if (track.Track is { } piece) {
                piece.ApplyStyle(style);
                if (piece.SupportsLabel) piece.SetLabel(_viewModel.Label);
                AddTrackPiece(track, grid);
                if (_drawCompass) AddCompass(track, grid);
            }
        }
        // DrawGridLines();
    }

    private void AddCompass(TrackPiece track, Grid grid) {
        var compass = TrackImages.Create("Track_Compass");
        if (compass is not null && track.Track is not null) {

            SetCompassColor(compass, "CompassN", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[0]);
            SetCompassColor(compass, "CompassNE", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[1]);
            SetCompassColor(compass, "CompassE", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[2]);
            SetCompassColor(compass, "CompassSE", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[3]);
            SetCompassColor(compass, "CompassS", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[4]);
            SetCompassColor(compass, "CompassSW", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[5]);
            SetCompassColor(compass, "CompassW", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[6]);
            SetCompassColor(compass, "CompassNW", track.Track.Connections.ConnectionPointsRotated(_viewModel.Rotation)[7]);
            
            var image = new Image {
                Source = compass.Image,
                HeightRequest = _viewModel.ComponentHeight,
                WidthRequest = _viewModel.ComponentWidth,
                ZIndex = 10
            };

            grid.Children.Add(image);
            grid.SetColumn(image, track.Col);
            grid.SetRow(image, track.Row);
        }
    }

    private void SetCompassColor(TrackImage compass, string compassId, TrackConnectionsEnum connection) {
        switch (connection) {
        case TrackConnectionsEnum.Terminator:
            compass.SetElementAttribute(compassId, "Color", Colors.Yellow.ToRgbaHex());
            compass.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Straight:
            compass.SetElementAttribute(compassId, "Color", Colors.Blue.ToRgbaHex());
            compass.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Closed:
            compass.SetElementAttribute(compassId, "Color", Colors.Green.ToRgbaHex());
            compass.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Diverging:
            compass.SetElementAttribute(compassId, "Color", Colors.Red.ToRgbaHex());
            compass.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Connector:
            compass.SetElementAttribute(compassId, "Color", Colors.Magenta.ToRgbaHex());
            compass.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.None:
        default:
            compass.SetElementAttribute(compassId, "Color", Colors.Transparent.ToRgbaHex());
            compass.SetElementAttribute(compassId, "Opacity", "0");
            break;
        }
    }

    private void AddTrackPiece(TrackPiece track, Grid grid) {
        var image = new Image();
        image.Source = track?.Track?.Image;
        image.HeightRequest = _viewModel.ComponentHeight;
        image.WidthRequest = _viewModel.ComponentWidth;
        image.Rotation = _viewModel.Rotation;
        image.ZIndex = 0;
        grid.Children.Add(image);
        grid.SetColumn(image,track.Col);
        grid.SetRow(image, track.Row);
    }

    private void Mainline_Button_OnClicked(object? sender, EventArgs e) {
        style = "Mainline";
        BuildTrackPieceGrid();
    }

    private void Branchline_Button_OnClicked(object? sender, EventArgs e) {
        style = "Branchline";
        BuildTrackPieceGrid();
    }

    private void MainlineHidden_Button_OnClicked(object? sender, EventArgs e) {
        style = "MainlineDashed";
        BuildTrackPieceGrid();
    }

    private void BranchlineHidden_Button_OnClicked(object? sender, EventArgs e) {
        style = "BranchlineDashed";
        BuildTrackPieceGrid();
    }

    private void CompassOn_Button_OnClicked(object? sender, EventArgs e) {
        _drawCompass = !_drawCompass;
        BuildTrackPieceGrid();
    }

    private void Slider_OnValueChanged(object? sender, ValueChangedEventArgs e) {
        BuildTrackPieceGrid();
    }

    private void Rotate_Button_OnClicked(object? sender, EventArgs e) {
        _viewModel.Rotation += 45;
        if (_viewModel.Rotation >= 360) _viewModel.Rotation = 0;
        BuildTrackPieceGrid();
    }

    private void Export_Button_OnClicked(object? sender, EventArgs e) {
        var json = TrackStyles.Export();
        TrackStyles.Import(json);
    }
    
    private void Button_OnClicked(object? sender, EventArgs e) {
        _factor = _factor < 1.5 ? 1.5 : 1.0;
        BuildTrackPieceGrid();
    }

    private void BindableObject_OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (_viewModel == null || _viewModel.Tracks == null) return;
        foreach (var track in _viewModel.Tracks) {
            if (track.Track is { SupportsLabel: true } piece) piece.SetLabel(_viewModel.Label ?? "");
        }
    }
}