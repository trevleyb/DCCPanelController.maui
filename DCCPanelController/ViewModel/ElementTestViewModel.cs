using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.TrackImages;
using DCCPanelController.Components.Tracks;
using TrackImage = DCCPanelController.Components.TrackImages.TrackImage;

namespace DCCPanelController.ViewModel;

public partial class ElementTestViewModel : BaseViewModel {

    [ObservableProperty] private List<TrackPiece> _tracks;
    [ObservableProperty] private int _scale = 10;
    [ObservableProperty] private int _componentWidth = 48;
    [ObservableProperty] private int _componentHeight = 48;
    [ObservableProperty] private int _rotation = 0;
    [ObservableProperty] private string _label;

    public ElementTestViewModel() {
        BuildTrackPiecesCollection();
    }

    /// <summary>
    /// Build up a sample of each of the Track Pieces
    /// </summary>
    public void BuildTrackPiecesCollection() {
        Tracks = [];
        var col = 0;
        var row = 0;
        foreach (var track in TrackImages.AvailableTracks.Where(track => !track.Key.Contains("Compass"))) {
            Tracks.Add(new TrackPiece(track.Value.Create, col, row));
            col++;
            if (col > 5) {
                col = 0; row++;
            }
        }
    }
    
}

[DebuggerDisplay("{Track?.Name}")]
public class TrackPiece (TrackImage track, int col, int row) {
    public TrackImage? Track { get; set; } = track;
    public int Col { get; set; } = col;
    public int Row { get; set; } = row;
}