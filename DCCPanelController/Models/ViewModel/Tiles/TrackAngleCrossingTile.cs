using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackAngleCrossingTile : TrackTile {
    public TrackAngleCrossingTile(AngleCrossingEntity entity, double gridSize) : base(entity, gridSize) { }
    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("angle", Entity.Rotation);
}