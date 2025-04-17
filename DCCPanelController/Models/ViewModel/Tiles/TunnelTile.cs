using System.Net.Http.Headers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class TunnelTile : TrackTile, ITileInteractive {
    public TunnelTile(TunnelEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("tunnel", Entity.Rotation);
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("tunnel", Entity.Rotation, SymbolScaleFactor);
    }
    
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
                _ => tunnel.TrackAttribute,
            };
        }
    }
}