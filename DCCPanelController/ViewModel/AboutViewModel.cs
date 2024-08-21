using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Tracks;
using TrackImage = DCCPanelController.Components.TrackImages.TrackImage;

namespace DCCPanelController.ViewModel;

public partial class AboutViewModel : BaseViewModel {
    
    [ObservableProperty] private List<TrackPiece> tracks;
    [ObservableProperty] public int scale = 10;
    [ObservableProperty] public int componentWidth = 48;
    [ObservableProperty] public int componentHeight = 48;
    [ObservableProperty] public int rotation = 0;

    public AboutViewModel() {
        BuildTrackPiecesCollection();
    }

    /// <summary>
    /// Build up a sample of each of the Track Pieces
    /// </summary>
    public void BuildTrackPiecesCollection() {
        tracks = new List<TrackPiece>();
        var col = 0;
        var row = 0;
        foreach (var track in TrackImages.AvailableTracks) {
            if (track.Key.Contains("Compass")) continue;
            tracks.Add(new TrackPiece(track.Value.Create, col, row));
            col++;
            if (col > 5) {
                col = 0; row++;
            }
        }
    }
    
}

public class TrackPiece (TrackImage track, int col, int row) {
    public TrackImage? Track { get; set; } = track;
    public int Col { get; set; } = col;
    public int Row { get; set; } = row;
}