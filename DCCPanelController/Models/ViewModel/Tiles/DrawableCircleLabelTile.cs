using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleLabelTile : Tile, ITileDrawable {
    public DrawableCircleLabelTile(CircleLabelEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not CircleLabelEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new CircleLabelDrawable(e)
        };
        gv.SetBinding(ScaleProperty, new Binding(nameof(e.Scale), source: e));
        gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class CircleLabelDrawable : IDrawable {
        private readonly CircleLabelEntity e;
        public CircleLabelDrawable(CircleLabelEntity e) => this.e = e;

        public void Draw(ICanvas canvas, RectF r) {
            canvas.SaveState();
            canvas.Antialias = true;

            // Outer stroke
            var outerStroke = (float)Math.Max(0, e.BorderWidth);
            if (outerStroke > 0f) {
                canvas.StrokeColor = e.BorderColor;
                canvas.StrokeSize = outerStroke;
                canvas.DrawEllipse(r.Inflate(new SizeF(-outerStroke / 2f)));
            }

            // Gap ring (white stroke inside)
            var gap = (float)Math.Max(0, e.BorderInnerGap);
            if (gap > 0f) {
                var gapRect = r.Inflate(new SizeF(-outerStroke - gap / 2f));
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = gap;
                canvas.DrawEllipse(gapRect);
            }

            // Inner circle (fill + inner border)
            var innerInset = outerStroke + gap;
            var innerRect = r.Inflate(new SizeF(-innerInset));
            if (e.BackgroundColor is { } fill) {
                canvas.FillColor = fill;
                canvas.FillEllipse(innerRect);
            }

            var innerStroke = (float)Math.Max(0, e.BorderInnerWidth);
            if (innerStroke > 0f) {
                canvas.StrokeColor = e.BorderInnerColor;
                canvas.StrokeSize = innerStroke;
                canvas.DrawEllipse(innerRect.Inflate(new SizeF(-innerStroke / 2f)));
            }

            // Centered text
            if (!string.IsNullOrWhiteSpace(e.Label)) {
                var font = e.FontStyle switch {
                    TextAttributeEnum.Regular         => "OpenSansRegular",
                    TextAttributeEnum.Bold            => "OpenSansBold",
                    TextAttributeEnum.BoldItalic      => "OpenSansBoldItalic",
                    TextAttributeEnum.ExtraBold       => "OpenSansExtraBold",
                    TextAttributeEnum.ExtraBoldItalic => "OpenSansExtraBoldItalic",
                    TextAttributeEnum.Italic          => "OpenSansItalic",
                    TextAttributeEnum.Light           => "OpenSansLight",
                    TextAttributeEnum.LightItalic     => "OpenSansLightItalic",
                    TextAttributeEnum.Medium          => "OpenSansMedium",
                    TextAttributeEnum.MediumItalic    => "OpenSansMediumItalic",
                    TextAttributeEnum.SemiBold        => "OpenSansSemiBold",
                    TextAttributeEnum.SemiBoldItalic  => "OpenSansSemiBoldItalic",
                    _                                 => "OpenSans",
                };

                canvas.FontSize = (float)e.FontSize;
                canvas.FontColor = e.TextColor;
                canvas.Font = new Microsoft.Maui.Graphics.Font(font);
                canvas.DrawString(e.Label, innerRect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            canvas.RestoreState();
        }
    }

    // protected override Microsoft.Maui.Controls.View? CreateTile() {
    //     var grid = new Grid {
    //         WidthRequest = TileWidth,
    //         HeightRequest = TileHeight,
    //     };
    //
    //     if (Entity is CircleLabelEntity entity) {
    //         var outerCircle = new Ellipse {
    //             Fill = Colors.Transparent,
    //             WidthRequest = TileWidth,
    //             HeightRequest = TileHeight,
    //             HorizontalOptions = LayoutOptions.Center,
    //             VerticalOptions = LayoutOptions.Center,
    //             ZIndex = entity.Layer,
    //             InputTransparent = true,
    //         };
    //
    //         outerCircle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: Entity));
    //         outerCircle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.OneWay, source: Entity));
    //         outerCircle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: Entity));
    //
    //         var circleGap = new Ellipse {
    //             Stroke = Colors.White,
    //             BackgroundColor = Colors.Transparent,
    //             WidthRequest = TileWidth - entity.BorderWidth,
    //             HeightRequest = TileWidth - entity.BorderWidth,
    //             HorizontalOptions = LayoutOptions.Center,
    //             VerticalOptions = LayoutOptions.Center,
    //             ZIndex = entity.Layer,
    //         };
    //         circleGap.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderInnerGap), BindingMode.OneWay, source: Entity));
    //
    //         var innerCircle = new Ellipse {
    //             WidthRequest = TileWidth - (entity.BorderWidth + entity.BorderInnerGap),
    //             HeightRequest = TileHeight - (entity.BorderWidth + entity.BorderInnerGap),
    //             HorizontalOptions = LayoutOptions.Center,
    //             VerticalOptions = LayoutOptions.Center,
    //             ZIndex = entity.Layer,
    //             InputTransparent = true,
    //         };
    //         innerCircle.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: Entity));
    //         innerCircle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderInnerColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: Entity));
    //         innerCircle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderInnerWidth), BindingMode.OneWay, source: Entity));
    //         innerCircle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: Entity));
    //
    //         var label = new Label {
    //             BackgroundColor = Colors.Transparent,
    //             ZIndex = entity.Layer,
    //             HorizontalOptions = LayoutOptions.Center,
    //             VerticalOptions = LayoutOptions.Center,
    //         };
    //         label.SetBinding(Label.FontSizeProperty, new Binding(nameof(entity.FontSize), BindingMode.OneWay, source: Entity));
    //         label.SetBinding(Label.TextColorProperty, new Binding(nameof(entity.TextColor), BindingMode.OneWay, source: Entity));
    //         label.SetBinding(Label.TextProperty, new Binding(nameof(entity.Label), BindingMode.OneWay, source: Entity));
    //
    //         grid.Children.Add(outerCircle);
    //         grid.Children.Add(innerCircle);
    //         grid.Children.Add(circleGap);
    //         grid.Children.Add(label);
    //         grid.SetBinding(ScaleProperty, new Binding(nameof(entity.Scale), BindingMode.OneWay, source: Entity));
    //         grid.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: Entity));
    //         return grid;
    //     }
    //     throw new TileRenderException(this.GetType(), Entity.GetType());
    // }
}