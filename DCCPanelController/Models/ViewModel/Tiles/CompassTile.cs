using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.TileCache;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CompassTile : Tile {
    public CompassTile(CompassEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        try {
            var svgImage = SvgImages.GetImage("compass", Entity.Rotation);
            var image = new Image { Source = svgImage.AsImageSource(0, DefaultScaleFactor), Scale = 1.5 };
            return image;
        } catch {
            throw new TileRenderException(this.GetType(), Entity.GetType());
        }
    }
}