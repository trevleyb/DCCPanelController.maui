using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Graphics.Text;

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

    protected override Microsoft.Maui.Controls.View CreateTile() {
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
        e.PropertyChanged += (_, _) => gv.Invalidate();
        return gv;
    }

    private sealed class TextDrawable(TextEntity e) : IDrawable {
        public void Draw(ICanvas canvas, RectF r) {
            if (string.IsNullOrWhiteSpace(e.Label))
                return;

            canvas.SaveState();
            canvas.Antialias = true;

            // --- Background ---
            if (e.BackgroundColor is { } bg) {
                canvas.FillColor = bg;
                canvas.FillRectangle(r);
            }

            // --- Font setup ---
            var alias = e.FontAlias ?? FontCatalog.DefaultFontAlias;
            var font = new Microsoft.Maui.Graphics.Font(alias);
            var text = e.Label;

            float requestedSize = e.FontSize <= 0 ? 14f : e.FontSize;
            canvas.Font = font;
            canvas.FontColor = e.TextColor;

            // --- Measure text size at requested font size ---
            var measured = canvas.GetStringSize(text, font, requestedSize);
            if (measured.Width <= 0 || measured.Height <= 0) {
                canvas.RestoreState();
                return;
            }

            // --- Compute rotated bounding box for scaling ---
            var radians = (float)(Math.Abs(e.Rotation % 360) * Math.PI / 180.0);
            var cos = Math.Abs((float)Math.Cos(radians));
            var sin = Math.Abs((float)Math.Sin(radians));
            var rotatedW = measured.Width * cos + measured.Height * sin;
            var rotatedH = measured.Width * sin + measured.Height * cos;

            const float pad = 2f;
            var scale = Math.Min(
                (r.Width - pad * 2) / rotatedW,
                (r.Height - pad * 2) / rotatedH);

            // Apply scale if needed
            float finalFontSize = requestedSize * Math.Min(1f, scale);
            if (finalFontSize < 4f) finalFontSize = 4f; // prevent disappearing

            canvas.FontSize = finalFontSize;

            // --- Apply rotation about the center of the rect ---
            canvas.Translate(r.Center.X, r.Center.Y);
            canvas.Rotate(e.Rotation % 360);
            canvas.Translate(-r.Center.X, -r.Center.Y);

            // --- Alignment setup ---
            var ha = EnumHelpers.ConvertHorizontalAlignmentToDraw(e.HorizontalJustification);
            var va = EnumHelpers.ConvertVerticalAlignmentToDraw(e.VerticalJustification);

            // --- Expand rect slightly to avoid clipping when rotated ---
            var expanded = new RectF(
                r.X - r.Width,
                r.Y - r.Height,
                r.Width * 3,
                r.Height * 3);

            // --- Draw string using the valid overload ---
            canvas.DrawString(text, expanded, ha, va);

            canvas.RestoreState();
        }
    }
}