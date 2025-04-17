using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CompassTile : Tile {
    public CompassTile(CompassEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        var svgImage = SvgImages.GetImage("compass", Entity.Rotation);
        var image = new Image { Source = svgImage.AsImageSource(0, DefaultScaleFactor), Scale = 1.5 };
        return image;
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("compass").AsImage();
    }
}