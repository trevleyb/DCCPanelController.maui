using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleLabelTile : Tile, ITileDrawable {
    public DrawableCircleLabelTile(CircleLabelEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(CircleLabelEntity.BackgroundColor));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderColor));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderWidth));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderInnerColor));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderInnerGap));
        VisualProperties.Add(nameof(CircleLabelEntity.BorderInnerWidth));
        VisualProperties.Add(nameof(CircleLabelEntity.FontSize));
        VisualProperties.Add(nameof(CircleLabelEntity.Label));
        VisualProperties.Add(nameof(CircleLabelEntity.TextColor));
        VisualProperties.Add(nameof(CircleLabelEntity.Scale));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        var grid = new Grid {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight
        };

        if (Entity is CircleLabelEntity entity) {
            var outerCircle = new Ellipse {
                Fill = Colors.Transparent,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = entity.Layer,
                InputTransparent = true
            };
            outerCircle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: Entity));
            outerCircle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: Entity));
            outerCircle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: Entity));

            var circleGap = new Ellipse {
                Stroke = Colors.White,
                BackgroundColor = Colors.Transparent,
                WidthRequest = TileWidth - entity.BorderWidth,
                HeightRequest = TileWidth - entity.BorderWidth,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = entity.Layer
            };
            circleGap.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderInnerGap), BindingMode.TwoWay, source: Entity));

            var innerCircle = new Ellipse {
                WidthRequest = TileWidth - (entity.BorderWidth + entity.BorderInnerGap),
                HeightRequest = TileHeight - (entity.BorderWidth + entity.BorderInnerGap),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                ZIndex = entity.Layer,
                InputTransparent = true
            };
            innerCircle.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: Entity));
            innerCircle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderInnerColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: Entity));
            innerCircle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderInnerWidth), BindingMode.TwoWay, source: Entity));
            innerCircle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: Entity));

            var label = new Label {
                BackgroundColor = Colors.Transparent,
                ZIndex = entity.Layer,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            label.SetBinding(Label.FontSizeProperty, new Binding(nameof(entity.FontSize), BindingMode.TwoWay, source: Entity));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(entity.TextColor), BindingMode.TwoWay, source: Entity));
            label.SetBinding(Label.TextProperty, new Binding(nameof(entity.Label), BindingMode.TwoWay, source: Entity));

            grid.Children.Add(outerCircle);
            grid.Children.Add(innerCircle);
            grid.Children.Add(circleGap);
            grid.Children.Add(label);
            grid.SetBinding(ScaleProperty, new Binding(nameof(entity.Scale), BindingMode.TwoWay, source: Entity));
            grid.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.TwoWay, source: Entity));
            return grid;
        }

        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("label").AsImage();
    }
}