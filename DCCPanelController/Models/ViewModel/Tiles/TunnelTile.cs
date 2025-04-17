using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TunnelTile : TrackTile, ITileInteractive {
    public TunnelTile(TunnelEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    public void Interact() {
        if (Entity is TunnelEntity tunnel) {
            tunnel.TrackType = tunnel.TrackType == TrackTypeEnum.MainLine ? TrackTypeEnum.BranchLine : TrackTypeEnum.MainLine;
        }
    }

    public void Secondary() {
        if (Entity is TunnelEntity tunnel) {
            tunnel.TrackAttribute = tunnel.TrackAttribute switch {
                TrackAttributeEnum.Normal => TrackAttributeEnum.Dashed,
                TrackAttributeEnum.Dashed => TrackAttributeEnum.Normal,
                _                         => tunnel.TrackAttribute
            };
        }
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("tunnel", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("tunnel", Entity.Rotation, SymbolScaleFactor);
    }
}