using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableTextTile : Tile, ITileDrawable {
    public DrawableTextTile(TextEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TextEntity.Label));
        VisualProperties.Add(nameof(entity.Label));
        VisualProperties.Add(nameof(entity.FontSize));
        VisualProperties.Add(nameof(entity.TextColor));
        VisualProperties.Add(nameof(entity.HorizontalJustification));
        VisualProperties.Add(nameof(entity.VerticalJustification));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is TextEntity entity && !string.IsNullOrEmpty(entity.Label)) {
            var label = new Label {
                //Text = entity.Label,
                //FontSize = entity.FontSize,
                HorizontalTextAlignment = entity.HorizontalJustification,
                VerticalTextAlignment = entity.VerticalJustification,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,

                //TextColor = entity.TextColor,
                //BackgroundColor = BackgroundColor,
                ZIndex = entity.Layer,

                //RotationX = entity.Rotation,
                LineBreakMode = LineBreakMode.TailTruncation,
                InputTransparent = true,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight
            };
            label.SetBinding(RotationProperty, new Binding(nameof(entity.Rotation), BindingMode.TwoWay, source: Entity));
            label.SetBinding(Label.FontSizeProperty, new Binding(nameof(entity.FontSize), BindingMode.TwoWay, source: Entity));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(entity.TextColor), BindingMode.TwoWay, source: Entity));
            label.SetBinding(Label.TextProperty, new Binding(nameof(entity.Label), BindingMode.TwoWay, source: Entity));
            label.SetBinding(BackgroundColorProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.TwoWay, source: Entity));
            label.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.TwoWay, source: entity));

            var border = new Border {
                Content = label,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight
            };
            border.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            border.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: entity));
            border.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: entity));
            border.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: entity));
            border.SetBinding(RoundRectangle.CornerRadiusProperty, new Binding(nameof(entity.BorderRadius), BindingMode.TwoWay, source: entity));
            return border;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("text").AsImage();
    }
}