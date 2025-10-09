using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
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

    private sealed class CircleLabelDrawable(CircleLabelEntity e) : IDrawable {
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
            var alias = e.FontAlias ?? "OpenSansRegular";
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

            // --- Expand rect slightly to avoid clipping when rotated ---
            var expanded = new RectF(
                r.X - r.Width,
                r.Y - r.Height,
                r.Width * 3,
                r.Height * 3);

            // --- Draw string using the valid overload ---
            canvas.DrawString(text, expanded, HorizontalAlignment.Center, VerticalAlignment.Center);

            canvas.RestoreState();
        }
    }
}