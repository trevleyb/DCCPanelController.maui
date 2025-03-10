using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class CompassTile(Entity entity, double gridSize) : Tile(entity, gridSize) {
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        var svgImage = SvgImages.GetImage("compass", Entity.Rotation);
        var image = new Image();
        image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
        return image;
    }
}
