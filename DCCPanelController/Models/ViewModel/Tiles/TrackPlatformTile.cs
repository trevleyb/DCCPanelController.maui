using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackPlatformTile : TrackTile {
    public TrackPlatformTile(PlatformEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(PlatformEntity.PlatformColor));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is PlatformEntity platform) {
            var style = new SvgStyleBuilder();
            style.Add(e => e.WithName(SvgElementType.Platform).WithColor(platform.PlatformColor ?? platform.Parent?.MainLineColor ?? Colors.Gray ));
            return CreateTrackTile("platform", Entity.Rotation, style.Build());
        }
        return CreateTrackTile("platform", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("platform", Entity.Rotation, SymbolScaleFactor);
    }
}