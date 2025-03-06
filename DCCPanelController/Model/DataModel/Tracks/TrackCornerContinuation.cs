using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;
public partial class TrackCornerContinuation : Track {
    public override string Name => "Corner Track";
    [ObservableProperty] private TerminatorStyleEnum _continuationStyle = TerminatorStyleEnum.Arrow;
    
    public TrackCornerContinuation() {}

    public TrackCornerContinuation(TrackCornerContinuation track) : base(track) {
        ContinuationStyle = track.ContinuationStyle;
    }
}