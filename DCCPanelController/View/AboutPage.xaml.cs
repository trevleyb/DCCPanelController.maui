using System.Collections.ObjectModel;
using DCCPanelController.Components.TrackImages;
using DCCPanelController.Components.Tracks;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;
using TrackImage = DCCPanelController.Components.TrackImages.TrackImage;

namespace DCCPanelController.View;

public partial class AboutPage : ContentPage {

    private AboutViewModel viewModel;
    private readonly ObservableCollection<Line> _lines = [];
    private double _factor = 1;
    private bool _drawCompass = false;
    private string style = "";
    
    public AboutPage() {
        InitializeComponent();
        viewModel = new AboutViewModel();
        BindingContext = viewModel;
        BuildTrackPieceGrid();
    }

    private void BuildTrackPieceGrid() {
        var grid = TestPiecesGrid;
        grid.Children.Clear();

        var gridSize = viewModel.Scale * 16;
        for (var i = 0; i < 6; i++) TestPiecesGrid.RowDefinitions[i].Height = new GridLength(gridSize); 
        for (var j = 0; j < 6; j++) TestPiecesGrid.ColumnDefinitions[j].Width = new GridLength(gridSize);
        viewModel.ComponentWidth = (int)(gridSize * _factor);
        viewModel.ComponentHeight = (int)(gridSize * _factor);

        foreach (var track in viewModel.Tracks) {
            if (track.Track is { } piece) {
                piece.ApplyStyle(style);
                if (piece.SupportsLabel) piece.SetLabel(viewModel.Label);
                AddTrackPiece(track, grid);
                if (_drawCompass) AddCompass(track, grid);
            }
        }
        // DrawGridLines();
    }

    private void AddCompass(TrackPiece track, Grid grid) {
        var compass = TrackImages.Create("Track_Compass");
        if (compass is not null) {

            SetCompassColor(compass, "CompassN", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[0]);
            SetCompassColor(compass, "CompassNE", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[1]);
            SetCompassColor(compass, "CompassE", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[2]);
            SetCompassColor(compass, "CompassSE", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[3]);
            SetCompassColor(compass, "CompassS", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[4]);
            SetCompassColor(compass, "CompassSW", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[5]);
            SetCompassColor(compass, "CompassW", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[6]);
            SetCompassColor(compass, "CompassNW", track.Track.Connections.ConnectionPointsRotated(viewModel.Rotation)[7]);
            
            var image = new Image {
                Source = compass.Image,
                HeightRequest = viewModel.ComponentHeight,
                WidthRequest = viewModel.ComponentWidth,
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
        image.HeightRequest = viewModel.ComponentHeight;
        image.WidthRequest = viewModel.ComponentWidth;
        image.Rotation = viewModel.Rotation;
        image.ZIndex = 0;
        grid.Children.Add(image);
        grid.SetColumn(image,track.Col);
        grid.SetRow(image, track.Row);
    }

    private void DrawGridLines() {
        
        var grid = TestPiecesGrid;
        if (_lines.Any()) {
            var removeLines = _lines.ToList();
            foreach (var line in removeLines) {
                grid.Children.Remove(line);
                _lines.Remove(line);
            }
        }
        
        var gridSize = viewModel.Scale * 16;
        var height = gridSize * grid.ColumnDefinitions.Count;
        var width =  gridSize * grid.RowDefinitions.Count;

        for (var i = 1; i < grid.ColumnDefinitions.Count; i++) {
            AddGridLine(grid, i * gridSize, i * gridSize, 0, height);
        }
        for (var i = 1; i < grid.RowDefinitions.Count; i++) {
            AddGridLine(grid, 0, width, i * gridSize, i * gridSize);
        }
    }
    
    private void AddGridLine(Grid grid, int x1, int x2, int y1, int y2) {
        var line = new Line() {
            X1 = x1, X2 = x2, Y1 = y1, Y2 = y2,
            IsEnabled = false, 
            ZIndex = 5, 
            Stroke = Colors.DarkGray, 
            StrokeThickness = 1,
        };

        _lines.Add(line);
        grid.Children.Add(line);
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
        viewModel.Rotation += 90;
        if (viewModel.Rotation >= 360) viewModel.Rotation = 0;
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
}

