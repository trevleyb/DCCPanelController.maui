using System.Net.Http.Headers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class StraightTile(Entity entity, double gridSize) : TrackTile(entity, gridSize), ITileInteractive {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("straight", Entity.Rotation);
    }

    public void Interact() {
        if (Entity is StraightEntity straight) {
            straight.TrackType = straight.TrackType == TrackTypeEnum.MainLine ? TrackTypeEnum.BranchLine : TrackTypeEnum.MainLine;
        }
    }

    public void Secondary() {
        if (Entity is StraightEntity straight) {
            straight.TrackAttribute = straight.TrackAttribute == TrackAttributeEnum.Normal ? TrackAttributeEnum.Hidden : TrackAttributeEnum.Normal;
        }
    }
}