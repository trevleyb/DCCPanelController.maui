using System.Net.Mime;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using ExCSS;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableTextTile : Tile, ITileDrawable {
    public DrawableTextTile(TextEntity entity, double gridSize) : base(entity, gridSize) {
        Watch
           .Track(nameof(TextEntity.HorizontalJustification), () => entity.HorizontalJustification)
           .Track(nameof(TextEntity.VerticalJustification), () => entity.VerticalJustification)
           .Track(nameof(TextEntity.BackgroundColor), () => entity.BackgroundColor)
           .Track(nameof(TextEntity.FontSize), () => entity.FontSize)
           .Track(nameof(TextEntity.FontStyle), () => entity.FontStyle)
           .Track(nameof(TextEntity.Label), () => entity.Label)
           .Track(nameof(TextEntity.TextColor), () => entity.TextColor);
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
}