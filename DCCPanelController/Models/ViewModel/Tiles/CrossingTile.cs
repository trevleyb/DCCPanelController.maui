using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class CrossingTile(Entity entity, double gridSize) : TrackTile(entity, gridSize) {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("cross", Entity.Rotation);
    }
}