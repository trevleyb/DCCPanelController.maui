using DCCPanelController.Components.Tracks.Base;
using DCCPanelController.Components.Tracks.ViewModels;
using DCCPanelController.Model;

namespace DCCPanelController.Components.Tracks;

public static class TrackViewModelFactory {
    
    public static ITrackViewModel GetViewModel(TrackTypesEnum trackType) {
        return trackType switch {
            TrackTypesEnum.StraightTrack => new StraightTrackViewModel(),
            TrackTypesEnum.Crossing      => new CrossTrackViewModel(),
            TrackTypesEnum.Terminator    => new TerminatorTrackViewModel(),
            TrackTypesEnum.LeftTrack     => new LeftTrackViewModel(),
            TrackTypesEnum.RightTrack    => new RightTrackViewModel(),
            TrackTypesEnum.LeftTurnout   => new LeftTurnoutViewModel(),
            TrackTypesEnum.RightTurnout  => new RightTurnoutViewModel(),
            TrackTypesEnum.WyeJunction   => new YJunctionViewModel(),
            _                            => new StraightTrackViewModel(),
        };
    }
    
    
}