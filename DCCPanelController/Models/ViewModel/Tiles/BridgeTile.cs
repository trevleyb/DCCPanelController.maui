using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class BridgeTile : TrackTile {
    public BridgeTile(BridgeEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(BridgeEntity.BridgeColor));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is BridgeEntity bridge) {
            var style = new SvgStyleBuilder();
            style.Add(e => e.WithName(SvgElementType.Bridge).WithColor(bridge.BridgeColor ?? bridge.Parent?.MainLineColor ?? Colors.Gray ));
            return CreateTrackTile("bridge", Entity.Rotation, style.Build());
        }
        return CreateTrackTile("bridge", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("bridge", Entity.Rotation, SymbolScaleFactor);
    }
}