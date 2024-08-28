using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.ViewModel;

public partial class ControlPanelViewModel : BaseViewModel {
    
    [ObservableProperty] private double _viewWidth;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _gridSize;

    [ObservableProperty] private bool _designMode = true;
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
    
    public void HandleTrackPieceTapped(ITrackPiece track) {
        if (track is ITrackInteractive) {
            switch (track) {
            case ITrackButton:
                Console.WriteLine($"You just tapped on {track.Name} - its a button so we will toggle it. ");
                track.NextState();
                break;
            case ITrackThreeway:
                Console.WriteLine($"You just tapped on {track.Name} - its a threeway so we will cycle states. ");
                track.NextState();
                break;
            case ITrackTurnout:
                Console.WriteLine($"You just tapped on {track.Name} - its turnout so we will cycle states. ");
                track.NextState();
                break;
            }
        } else {
            Console.WriteLine($"You just tapped on {track.Name} but it is not interactive so we will rotate it.");
            track.RotateLeft();
        }
        
        
    }
}