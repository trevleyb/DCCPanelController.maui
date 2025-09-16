using System.Net.Mime;
using AVFoundation;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using ExCSS;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableTextTile : Tile, ITileDrawable {
    public DrawableTextTile(TextEntity entity, double gridSize) : base(entity, gridSize) {
        VisualProperties.Add(nameof(entity.HorizontalJustification));
        VisualProperties.Add(nameof(entity.VerticalJustification));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not TextEntity e) throw new TileRenderException(GetType(), Entity.GetType());
        if (string.IsNullOrEmpty(e.Label)) e.Label = "T";

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new TextDrawable(e)
        };
        gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class TextDrawable : IDrawable {
        private readonly TextEntity e;
        public TextDrawable(TextEntity e) => this.e = e;

        public void Draw(ICanvas canvas, RectF r) {
            canvas.SaveState();
            canvas.Antialias = true;

            // Background
            if (e.BackgroundColor is { } bg) {
                canvas.FillColor = bg;
                canvas.FillRectangle(r);
            }

            // Text
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
                _ => "OpenSans",
            };

            canvas.FontColor = e.TextColor;
            canvas.FontSize = (float)e.FontSize;
            canvas.Font = new Microsoft.Maui.Graphics.Font(font);

            // Rotate around center
            canvas.Translate(r.Center.X, r.Center.Y);
            canvas.Rotate((float)(e.Rotation % 360));
            canvas.Translate(-r.Center.X, -r.Center.Y);

            var ha = EnumHelpers.ConvertHorizontalAlignmentToDraw(e.HorizontalJustification);
            var va = EnumHelpers.ConvertVerticalAlignmentToDraw(e.VerticalJustification);
            canvas.DrawString(e.Label ?? "T", r, ha, va);
            canvas.RestoreState();
        }
    }

    // protected override Microsoft.Maui.Controls.View? CreateTile() {
    //     if (Entity is TextEntity entity) {
    //         if (string.IsNullOrEmpty(entity.Label)) entity.Label = "T";
    //         var label = new Label {
    //             HorizontalTextAlignment = EnumHelpers.ConvertHorizontalAlignmentToText(entity.HorizontalJustification),
    //             VerticalTextAlignment = EnumHelpers.ConvertVerticalAlignmentToText(entity.VerticalJustification),
    //             HorizontalOptions = LayoutOptions.Fill,
    //             VerticalOptions = LayoutOptions.Fill,
    //             LineBreakMode = LineBreakMode.TailTruncation,
    //             InputTransparent = true,
    //             WidthRequest = TileWidth,
    //             HeightRequest = TileHeight,
    //             FontFamily = EnumHelpers.ConvertFontStyle(entity.FontStyle),
    //         };
    //         label.SetBinding(Label.BackgroundColorProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.OneWay, source: entity));
    //         label.SetBinding(Label.FontSizeProperty, new Binding(nameof(entity.FontSize), BindingMode.OneWay, source: entity));
    //         label.SetBinding(Label.TextColorProperty, new Binding(nameof(entity.TextColor), BindingMode.OneWay, source: entity));
    //         label.SetBinding(Label.RotationProperty, new Binding(nameof(entity.Rotation), BindingMode.OneWay, source: entity));
    //         label.SetBinding(Label.TextProperty, new Binding(nameof(entity.Label), BindingMode.OneWay, source: entity));
    //         label.SetBinding(Label.ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
    //         return label;
    //     }
    //     throw new TileRenderException(this.GetType(), Entity.GetType());
    // }
}