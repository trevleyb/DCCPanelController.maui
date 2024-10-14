using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private ObservableCollection<ITrackSymbol> _trackSymbols = [];
    [ObservableProperty] private Panel _panel;

    public PanelEditorViewModel(Panel panel) {
        _panel = panel;
        foreach (var symbol in TrackPieceFactory.TrackPieces) {
            if (symbol is ITrackSymbol trackSymbol) {
                TrackSymbols.Add(trackSymbol);
            }
        }
    }

    public bool TracksOutsideBounds => Panel.Tracks.Any(track => track.X < 0 || track.X >= Panel.Cols || track.Y < 0 || track.Y >= Panel.Rows);
    
    public void Validate() {
        // Make sure that all the Coordinates for the Track Pieces are valid and 
        // if not, make sure they are within the bounds of the Panel. 
        if (!Panel.Tracks.Any()) return;
        for (var idx = Panel.Tracks.Count -1; idx >= 0; idx--) {
            var track = Panel.Tracks[idx];
            if (track.X < 0 || track.X >= Panel.Cols || track.Y < 0 || track.Y >= Panel.Rows) {
                Panel.Tracks.Remove(track);
            }
        }
    }

    
}