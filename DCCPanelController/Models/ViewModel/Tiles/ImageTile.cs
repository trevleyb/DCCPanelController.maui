using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using ExCSS;
using Microsoft.Maui.Controls.Shapes;
using Colors = Microsoft.Maui.Graphics.Colors;
using Shape = Microsoft.Maui.Controls.Shapes.Shape;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class ImageTile : Tile {
    public ImageTile(ImageEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is ImageEntity entity && !string.IsNullOrEmpty(entity.Image)) {
            var image = new Image {
                Source = ImageHelper.ImageFromBase64(entity.Image),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                ZIndex = entity.Layer,
                InputTransparent = true,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight
            };
            image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            image.SetBinding(Image.AspectProperty, new Binding(nameof(entity.AspectRatio), BindingMode.OneWay, source: Entity));

            var border = new Border {
                Content = image,
                BackgroundColor = Colors.Transparent,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                ZIndex = entity.Layer,
                StrokeShape = new RoundRectangle {
                    CornerRadius = new CornerRadius(entity.BorderRadius)
                }
            };
            border.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            border.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: entity));
            border.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, converter: new ColorToSolidColorConverter(), source: entity));
            border.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: entity));
            border.SetBinding(RoundRectangle.CornerRadiusProperty, new Binding(nameof(entity.BorderRadius), BindingMode.TwoWay, converter: new CornerRadiusConverter(),  source: entity));
            return border;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("image").AsImage();
    }
}