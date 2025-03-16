using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.StyleManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CircleLabelTile : Tile {
    public CircleLabelTile(CircleLabelEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is CircleLabelEntity entity) {
            var svgImage = SvgImages.GetImage("label", Entity.Rotation);
            svgImage.SetAttribute(SvgElementType.ButtonOutline, entity.BorderColor);
            svgImage.SetAttribute(SvgElementType.Button, entity.BackgroundColor);
            svgImage.SetAttribute(SvgElementType.Text, entity.TextColor);
            svgImage.SetLabel(entity.Label);
            
            var image = new Image();
            image.Scale = 1.5;
            image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
            return image;
        }
        return CreateSymbol();
    }
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("label").AsImage();
    }
}