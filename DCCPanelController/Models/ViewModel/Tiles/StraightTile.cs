using System.Net.Http.Headers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class StraightTile : TrackTile, ITileInteractive {
    public StraightTile(StraightEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("straight", Entity.Rotation);
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("straight", Entity.Rotation, SymbolScaleFactor);
    }
    
    public void Interact() {
        if (Entity is StraightEntity straight) {
            straight.TrackType = straight.TrackType == TrackTypeEnum.MainLine ? TrackTypeEnum.BranchLine : TrackTypeEnum.MainLine;
        }
    }

    public void Secondary() {
        if (Entity is StraightEntity straight) {
            straight.TrackAttribute = straight.TrackAttribute switch {
                TrackAttributeEnum.Normal => TrackAttributeEnum.Dashed,
                TrackAttributeEnum.Dashed => TrackAttributeEnum.Normal,
                _ => straight.TrackAttribute,
            };
        }
    }
}