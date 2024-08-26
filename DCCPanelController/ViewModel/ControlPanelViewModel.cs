using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;

namespace DCCPanelController.ViewModel;

public partial class ControlPanelViewModel : BaseViewModel {
    
    [ObservableProperty] private double _viewWidth;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _gridSize;
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private Panel _panel;
    [ObservableProperty] private ObservableCollection<ITrackPiece> _trackPieces;
    [ObservableProperty] private Color _gridColor;

    public ControlPanelViewModel() {
        Panel = CreateTestPanel();
        TrackPieces = Panel.Tracks;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if ((bool)e?.PropertyName?.Equals(nameof(ShowGrid))) {
            GridColor = ShowGrid ? Colors.Green : Colors.Transparent;
            OnPropertyChanged(nameof(GridColor));
        }
    }

    public void SetScreenSize(double width, double height) {
        GridSize = (width > 0 && height > 0) ? (Math.Min(width / Panel.Cols, height / Panel.Rows) / 2) * 2 : 1;
        ViewWidth = GridSize * Panel.Cols;
        ViewHeight = GridSize * Panel.Rows;
    }
    
    private Panel CreateTestPanel() {
        var panel = new Panel();
        panel.Id = "01";
        panel.Name = "Test Panel";
        panel.Cols = 18;
        panel.Rows = 12;
        panel.Tracks = [];
        panel.Tracks.Add(new TrackStraight() { X = 1, Y = 1, ImageRotation = 0 });
        panel.Tracks.Add(new TrackStraight() { X = 2, Y = 2, ImageRotation = 90 });
        
        panel.Tracks.Add(new TrackCorner()   { X = 2, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 3, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 4, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 5, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 6, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 7, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 8, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackCorner()   { X = 9, Y = 3, ImageRotation = 0 });
        
        panel.Tracks.Add(new TrackStraight() { X = 4, Y = 4, ImageRotation = 270 });
        panel.Tracks.Add(new TrackStraight() { X = 5, Y = 5, ImageRotation = 0 });
        return panel;
    }
}