using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackStraightTile : TrackTile {
    public TrackStraightTile(StraightEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() =>
        ((StraightEntity)Entity).TrackStyle switch {
            TrackStyleEnum.Normal     => CreateTrackTile("straight", Entity.Rotation),
            TrackStyleEnum.Tunnel     => CreateTrackTile("tunnel", Entity.Rotation),
            TrackStyleEnum.Bridge     => CreateTrackTile("bridge", Entity.Rotation),
            TrackStyleEnum.Platform   => CreateTrackTile("platform", Entity.Rotation),
            TrackStyleEnum.Terminator => CreateTrackTile("terminator", Entity.Rotation),
            TrackStyleEnum.Lines      => CreateTrackTile("lines", Entity.Rotation),
            TrackStyleEnum.Arrow      => CreateTrackTile("arrow", Entity.Rotation),
            TrackStyleEnum.Rounded    => CreateTrackTile("rounded", Entity.Rotation),
            _                         => CreateTrackTile("straight", Entity.Rotation),
        };

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTile(); 
    
}