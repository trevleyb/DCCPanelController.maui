using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackTerminator : Track {
    public override string Name => "Terminator Track";
    [ObservableProperty] private TerminatorStyleEnum _terminatorStyle = TerminatorStyleEnum.Arrow;

    public TrackTerminator() { }
    public TrackTerminator(TrackTerminator track) : base(track) {
        TerminatorStyle = track.TerminatorStyle;
    }
}