using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.StyleManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CircleLabelTile : Tile {
    public CircleLabelTile(CircleLabelEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(CircleLabelEntity.BorderColor));
        VisualProperties.Add(nameof(CircleLabelEntity.BackgroundColor));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderWidth));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderInnerGap));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderInnerColor));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderInnerWidth));
        VisualProperties.Add(nameof(CircleLabelEntity.FontSize));
        VisualProperties.Add(nameof(CircleLabelEntity.TextColor));
        VisualProperties.Add(nameof(CircleLabelEntity.Label));
        VisualProperties.Add(nameof(CircleLabelEntity.Opacity));
        VisualProperties.Add(nameof(CircleLabelEntity.Layer));
        VisualProperties.Add(nameof(CircleLabelEntity.ScaleFactor));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        // if (Entity is CircleLabelEntity entity) {
        //     var svgImage = SvgImages.GetImage("label", Entity.Rotation);
        //     svgImage.SetAttribute(SvgElementType.ButtonOutline, entity.BorderColor);
        //     svgImage.SetAttribute(SvgElementType.Button, entity.BackgroundColor);
        //     svgImage.SetAttribute(SvgElementType.Text, entity.TextColor);
        //     svgImage.SetLabel(entity.Label);
        //     
        //     var image = new Image();
        //     image.Scale = 1.5;
        //     image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
        //     image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
        //     return image;
        // }

        var grid = new Grid() {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
        };

        // Draw a Circle for the Outer Border
        // Draw a Circle for the Gap between the inner and outer circles
        // Draw a Circle for the main inner circle
        
        if (Entity is CircleLabelEntity entity) {
            var outerCircle = new Ellipse {
                Fill = Colors.Transparent,
                Stroke = entity.BorderColor,
                StrokeThickness = entity.BorderWidth,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = entity.Layer,
                Opacity = entity.Opacity,
                InputTransparent = true,
                Scale = 1
            };
            
            var circleGap = new Ellipse {
                Stroke = Colors.White,
                BackgroundColor = Colors.Transparent,
                StrokeThickness = entity.BorderInnerGap,
                WidthRequest = TileWidth - (entity.BorderWidth),
                HeightRequest = TileWidth - (entity.BorderWidth),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = entity.Layer,
            };
            var innerCircle = new Ellipse {
                Fill = entity.BackgroundColor ?? Colors.Black,
                Stroke = entity.BorderInnerColor ?? Colors.Black,
                StrokeThickness = entity.BorderInnerWidth,
                WidthRequest = TileWidth - (entity.BorderWidth + entity.BorderInnerGap),
                HeightRequest = TileHeight - (entity.BorderWidth + entity.BorderInnerGap),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = entity.Layer,
                Opacity = entity.Opacity,
                InputTransparent = true,
                Scale = 1
            };
            var label = new Label() {
                BackgroundColor = Colors.Transparent,
                FontSize = entity.FontSize,
                TextColor = entity.TextColor,
                Text = entity.Label,
                ZIndex = entity.Layer,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Children.Add(outerCircle);
            grid.Children.Add(innerCircle);
            grid.Children.Add(circleGap);
            grid.Children.Add(label);
            grid.Scale = entity.ScaleFactor;
            return grid;
        }

        return CreateSymbol();
    }
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("label").AsImage();
    }
}