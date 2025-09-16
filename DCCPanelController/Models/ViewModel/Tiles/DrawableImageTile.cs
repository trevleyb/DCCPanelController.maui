using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;
using Colors = Microsoft.Maui.Graphics.Colors;
using Shape = Microsoft.Maui.Controls.Shapes.Shape;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableImageTile : Tile, ITileDrawable {
    public DrawableImageTile(ImageEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(ImageEntity.Image));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not ImageEntity entity)
            throw new TileRenderException(GetType(), Entity.GetType());

        var image = new Image {
            Aspect = Aspect.AspectFill,
            InputTransparent = true,
            Source = "image.svg",
        };

        image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageEntity.Image), BindingMode.OneWay,
                new Base64ToImageSourceConverter(), source: entity));
        
        image.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), source: entity));
        image.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), source: entity));

        // If there’s no border, just return the image
        if (entity.BorderWidth <= 0 && entity.BorderRadius <= 0) return image;

        // Create a stroke shape instance we can actually bind to
        var rr = new RoundRectangle();

        // IMPORTANT: bind CornerRadius on the RoundRectangle, not on the Border
        rr.SetBinding(RoundRectangle.CornerRadiusProperty,
            new Binding(nameof(entity.BorderRadius), BindingMode.TwoWay,
                new CornerRadiusConverter(), source: entity));

        var border = new Border {
            Padding = 0,
            Background = Brush.Transparent, // helps ensure clipping to StrokeShape on iOS
            StrokeShape = rr,
            Content = image,
            InputTransparent = true
        };

        // Bind stroke color/thickness (Stroke is a Brush)
        border.SetBinding(Border.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: entity));
        border.SetBinding(Border.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: entity));

        // Tile-level transforms/opacity
        border.SetBinding(VisualElement.RotationProperty, new Binding(nameof(Rotation), source: this));
        border.SetBinding(VisualElement.OpacityProperty, new Binding(nameof(entity.Opacity), source: entity));
        border.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), source: entity));

        // Optional: clip child to the same round rect explicitly (belt & braces)
        // border.Clip = new RoundRectangleGeometry { CornerRadius = rr.CornerRadius };
        // If you set Clip, update its Rect on SizeChanged to match actual size.

        entity.PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(ImageEntity.Image)) {
                if (string.IsNullOrWhiteSpace(entity.Image)) image.Source = "image.svg"; // restore fallback
            }
        };
        return border;
    }

    // protected override Microsoft.Maui.Controls.View? CreateTile() {
    //     if (Entity is ImageEntity entity) {
    //         var image = new Image {
    //             Source = string.IsNullOrEmpty(entity.Image) ? SvgImages.GetImage("image").AsImageSource() : ImageHelper.ImageFromBase64(entity.Image),
    //             HorizontalOptions = LayoutOptions.Start,
    //             VerticalOptions = LayoutOptions.Start,
    //             ZIndex = entity.Layer,
    //             InputTransparent = true,
    //             WidthRequest = TileWidth,
    //             HeightRequest = TileHeight,
    //         };
    //         image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
    //         image.SetBinding(Image.AspectProperty, new Binding(nameof(entity.AspectRatio), BindingMode.OneWay, source: Entity));
    //         if (entity.BorderWidth <= 0) return image; // No border (no need to create a border)
    //
    //         var border = new Border {
    //             Content = image,
    //             BackgroundColor = Colors.Transparent,
    //             InputTransparent = true,
    //             HorizontalOptions = LayoutOptions.Start,
    //             VerticalOptions = LayoutOptions.Start,
    //             WidthRequest = TileWidth,
    //             HeightRequest = TileHeight,
    //             ZIndex = entity.Layer,
    //             StrokeShape = new RoundRectangle {
    //                 CornerRadius = new CornerRadius(entity.BorderRadius),
    //             },
    //         };
    //         border.SetBinding(Border.RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
    //         border.SetBinding(Border.OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: entity));
    //         border.SetBinding(Border.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: entity));
    //         border.SetBinding(Border.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: entity));
    //         border.SetBinding(RoundRectangle.CornerRadiusProperty, new Binding(nameof(entity.BorderRadius), BindingMode.TwoWay, new CornerRadiusConverter(), source: entity));
    //         border.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.TwoWay, source: entity));
    //         return border;
    //     }
    //     throw new TileRenderException(this.GetType(), Entity.GetType());
    // }
}