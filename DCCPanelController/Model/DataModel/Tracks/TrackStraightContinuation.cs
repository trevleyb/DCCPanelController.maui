using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackStraightContinuation : Track {
    public override string Name => "Straight Track";
    [ObservableProperty] private TerminatorStyleEnum _continuationStyle = TerminatorStyleEnum.Arrow;
}