using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class TerminatorTile(Entity entity, double gridSize) : TrackTile(entity, gridSize) {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("terminator", Entity.Rotation);
    }
}