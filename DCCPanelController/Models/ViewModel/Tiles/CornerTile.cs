using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class CornerTile(Entity entity, double gridSize) : TrackTile(entity, gridSize) {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("corner", Entity.Rotation);
    }
}