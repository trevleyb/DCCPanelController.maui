using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ImageTile : Tile {
    public ImageTile(ImageEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {

        if (Entity is ImageEntity entity && !string.IsNullOrEmpty(entity.Image)) {
            Microsoft.Maui.Controls.View view;
            var image = new Image {
                Source = ImageHelper.ImageFromBase64(entity.Image),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                ZIndex = entity.Layer,
                Aspect = entity.AspectRatio,
                RotationX = entity.Rotation,
                InputTransparent = true,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight
            };

            if (entity.BorderWidth > 0) {
                view = new Border {
                    Content = image,
                    InputTransparent = true,
                    RotationX = entity.Rotation,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    WidthRequest = TileWidth,
                    HeightRequest = TileHeight,
                    StrokeThickness = entity.BorderWidth,
                    Stroke = entity.BorderColor,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(entity.BorderRadius) }
                };
            } else view = image;
            view.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            return view;
        }
        return CreateSymbol();
    }
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("image").AsImage();
    }

}