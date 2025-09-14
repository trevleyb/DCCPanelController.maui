using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TunnelTile : TrackTile {
    public TunnelTile(TunnelEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) => VisualProperties.Add(nameof(TunnelEntity.TunnelColor));

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is TunnelEntity tunnel) {
            var style = new SvgStyleBuilder();
            style.Add(e => e.WithName(SvgElementType.Tunnel).WithColor(tunnel.TunnelColor ?? tunnel.Parent?.TunnelColor ?? Colors.Gray));
            return CreateTrackTile("tunnel", Entity.Rotation, style.Build());
        }
        return CreateTrackTile("tunnel", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("tunnel", Entity.Rotation, SymbolScaleFactor);
}