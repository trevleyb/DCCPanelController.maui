using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {
    [ObservableProperty] private Panel _panel;
    [ObservableProperty] private bool _hasSelectedTracks;

    public PanelEditorViewModel(Panel panel) {
        _panel = panel;
        PropertyChanged += OnPropertyChanged;
        Panel.PropertyChanged += OnPanelPropertyChanged;
        foreach (var symbol in TrackPieceFactory.TrackPieces) {
            if (symbol is ITrackSymbol trackSymbol) {
                TrackSymbols.Add(trackSymbol);
            }
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PanelEditorView: PropertyChanged: {sender} - {e.PropertyName}");
    }

    private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PanelEditorView: PanelPropertyChanged: {sender} - {e.PropertyName}");
    }

    public ObservableCollection<ITrackSymbol> TrackSymbols { get; set; } = [];
    public bool TracksOutsideBounds => Panel.Tracks.Any(track => track.X < 0 || track.X >= Panel.Cols || track.Y < 0 || track.Y >= Panel.Rows);
    public void Validate() {
        // Make sure that all the Coordinates for the Track Pieces are valid and 
        // if not, make sure they are within the bounds of the Panel. 
        if (!Panel.Tracks.Any()) return;
        for (var idx = Panel.Tracks.Count - 1; idx >= 0; idx--) {
            var track = Panel.Tracks[idx];
            if (track.X < 0 || track.X >= Panel.Cols || track.Y < 0 || track.Y >= Panel.Rows) {
                Panel.Tracks.Remove(track);
            }
        }
    }
}