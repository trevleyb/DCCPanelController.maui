using System.Net.Http.Headers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class PlatformTile : TrackTile, ITileInteractive {
    public PlatformTile(PlatformEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("platform", Entity.Rotation);
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("platform", Entity.Rotation, SymbolScaleFactor);
    }
    
    public void Interact() {
        if (Entity is PlatformEntity straight) {
            straight.TrackType = straight.TrackType == TrackTypeEnum.MainLine ? TrackTypeEnum.BranchLine : TrackTypeEnum.MainLine;
        }
    }

    public void Secondary() { }
}