using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;

namespace DCCPanelController.ViewModel;

public partial class ControlPanelViewModel : BaseViewModel {
    
    [ObservableProperty] private double _viewWidth;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _gridSize;
    
    [ObservableProperty] private bool _showGrid = true;
    [ObservableProperty] private Panel? _panel;
    [ObservableProperty] private Color _gridColor = Colors.DarkGrey;

    public string Name => Panel?.Name ?? "Unknown Panel";
    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;
    
    public ControlPanelViewModel(Panel panel) {
        Panel = panel;
    }
    
    public void SetScreenSize(double width, double height) {
        GridSize = (width > 0 && height > 0) ? (Math.Min(width / Cols, height / Rows) / 2) * 2 : 1;
        ViewWidth = GridSize * Cols;
        ViewHeight = GridSize * Rows;
    }

    [RelayCommand]
    private void TrackImageTapped(TrackPiece track) {
        Console.WriteLine($"You just tapped on {track.Name} which is in a state of {track.State}");
    }
}