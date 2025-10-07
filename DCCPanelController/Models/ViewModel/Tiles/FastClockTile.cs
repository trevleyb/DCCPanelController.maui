using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

/// <summary>
/// A single tile that draws either a Digital or Analog fast clock depending on FastClockEntity.FastclockType.
/// - Time source: Entity -> Parent (Panel) -> Panels -> Profile.FastClock
/// - Colors: BackgroundColor, BorderColor, HoursColor, TimeColor, SecondHandColor
/// - Non-rotatable: RotationFactor = 0 in FastClockEntity
/// </summary>
public sealed class FastClockTile : Tile, ITileDrawable {
    public FastClockTile(FastClockEntity entity, double gridSize) : base(entity, gridSize) { }

    private Profile? _profile;
    private GraphicsView? _gv;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not FastClockEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        // Locate the active profile (Entity -> Panel -> Panels -> Profile)
        _profile = (e.Parent as Panel)?.Panels?.Profile;

        _gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new FastClockDrawable(e, () => _profile?.FastClock ?? DateTime.Now)
        };
        _gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        _gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));

        // Redraw when FastClockEntity visuals OR type change
        e.PropertyChanged += OnEntityChanged;

        // Redraw when the fast clock ticks in the active profile
        if (_profile is not null) _profile.PropertyChanged += OnProfileChanged;

        return _gv;
    }

    protected override void Cleanup() {
        base.Cleanup();
        if (Entity is FastClockEntity e) e.PropertyChanged -= OnEntityChanged;
        if (_profile is not null) _profile.PropertyChanged -= OnProfileChanged;
        _gv = null;
        _profile = null;
    }

    private void OnEntityChanged(object? s, PropertyChangedEventArgs e) {
        // Any color/opacity/type/etc. change should redraw
        _gv?.Invalidate();
    }

    private void OnProfileChanged(object? s, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(Profile.FastClock)) _gv?.Invalidate();
    }

    private sealed class FastClockDrawable : IDrawable {
        private readonly FastClockEntity _e;
        private readonly Func<DateTime>  _now;

        public FastClockDrawable(FastClockEntity e, Func<DateTime> nowProvider) {
            _e   = e;
            _now = nowProvider;
        }

        public void Draw(ICanvas canvas, RectF dirty) {
            canvas.SaveState();
            canvas.Antialias = true;

            // Fill background
            canvas.FillColor = _e.BackgroundColor;
            canvas.FillRectangle(dirty);

            // Border (inset to keep stroke inside the tile)
            var border = Math.Max(1f, Math.Min(dirty.Width, dirty.Height) * 0.04f);
            canvas.StrokeColor = _e.BorderColor;
            canvas.StrokeSize  = border;
            var inset = dirty.Inflate(new SizeF(-border / 2f));
            canvas.DrawRectangle(inset);

            // Choose rendering mode based on entity enum
            if (_e.FastclockType == FastClockTypeEnum.Digital) {
                DrawDigital(canvas, inset);
            } else {
                DrawAnalog(canvas, inset);
            }

            canvas.RestoreState();
        }

        // -------------------------
        // DIGITAL: HH:mm:ss
        // -------------------------
        private void DrawDigital(ICanvas canvas, RectF area) {
            var now  = _now();
            var hhmm = now.ToString("HH:mm");
            var ss   = now.ToString("ss");

            // Layout paddings & areas
            var pad   = Math.Max(area.Width, area.Height) * 0.06f;
            var inner = area.Inflate(new SizeF(-pad, -pad));

            // Reserve ~75% width for HH:mm and ~25% for :ss
            var leftRect  = new RectF(inner.Left, inner.Top, inner.Width * 0.75f, inner.Height);
            var rightRect = new RectF(leftRect.Right, inner.Top, inner.Width - leftRect.Width, inner.Height);

            // Font sizes scale with tile size
            var baseSize    = Math.Min(inner.Width, inner.Height) * 0.65f;
            var secondsSize = baseSize * 0.55f;

            // HH:mm in TimeColor
            canvas.FontColor = _e.TimeColor;
            canvas.FontSize  = (float)baseSize;
            canvas.DrawString(hhmm, leftRect, HorizontalAlignment.Center, VerticalAlignment.Center);

            // :ss in SecondHandColor
            canvas.FontColor = _e.SecondHandColor;
            canvas.FontSize  = (float)secondsSize;
            canvas.DrawString(":" + ss, rightRect, HorizontalAlignment.Left, VerticalAlignment.Center);
        }

        // -------------------------
        // ANALOG
        // -------------------------
        private void DrawAnalog(ICanvas canvas, RectF area) {
            // Square fit
            var size   = Math.Min(area.Width, area.Height);
            var cx     = area.Left + area.Width / 2f;
            var cy     = area.Top  + area.Height / 2f;
            var radius = size * 0.48f;

            // Face
            var faceRect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            canvas.FillColor = _e.BackgroundColor;
            canvas.FillEllipse(faceRect);

            // Ring
            var stroke = Math.Max(1f, size * 0.04f);
            canvas.StrokeColor = _e.BorderColor;
            canvas.StrokeSize  = stroke;
            var ring = faceRect.Inflate(new SizeF(-stroke / 2f));
            canvas.DrawEllipse(ring);

            // Hour numbers (1..12) in HoursColor
            var fontSize = radius * 0.23f;
            canvas.FontColor = _e.HoursColor;
            canvas.FontSize  = fontSize;
            for (int h = 1; h <= 12; h++) {
                var ang = (float)((h / 12.0) * 2 * Math.PI - Math.PI / 2); // 12 at top
                var r   = radius * 0.78f;
                var tx  = cx + (float)(Math.Cos(ang) * r);
                var ty  = cy + (float)(Math.Sin(ang) * r);

                var w    = radius * 0.35f;
                var rect = new RectF(tx - w / 2f, ty - fontSize / 2f, w, fontSize * 1.1f);
                canvas.DrawString(h.ToString(), rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            // Minute/Hour ticks
            canvas.StrokeColor = _e.HoursColor.WithAlpha(0.5f);
            canvas.StrokeSize  = Math.Max(1f, size * 0.01f);
            for (int i = 0; i < 60; i++) {
                var isHour = i % 5 == 0;
                var r1     = radius * (isHour ? 0.86f : 0.92f);
                var r2     = radius * 0.98f;
                var a      = (float)(i / 60.0 * 2 * Math.PI - Math.PI / 2);
                var x1     = cx + (float)Math.Cos(a) * r1;
                var y1     = cy + (float)Math.Sin(a) * r1;
                var x2     = cx + (float)Math.Cos(a) * r2;
                var y2     = cy + (float)Math.Sin(a) * r2;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Time now
            var now    = _now();
            var hour   = now.Hour % 12 + now.Minute / 60.0 + now.Second / 3600.0;
            var minute = now.Minute + now.Second / 60.0;
            var second = now.Second + now.Millisecond / 1000.0;

            // Angles (12 at top)
            var ah = (float)(hour   / 12.0 * 2 * Math.PI - Math.PI / 2);
            var am = (float)(minute / 60.0 * 2 * Math.PI - Math.PI / 2);
            var as_ = (float)(second / 60.0 * 2 * Math.PI - Math.PI / 2);

            // Hour hand (TimeColor)
            canvas.StrokeColor = _e.TimeColor;
            canvas.StrokeSize  = Math.Max(2f, size * 0.035f);
            var hx = cx + (float)Math.Cos(ah) * radius * 0.55f;
            var hy = cy + (float)Math.Sin(ah) * radius * 0.55f;
            canvas.DrawLine(cx, cy, hx, hy);

            // Minute hand (TimeColor)
            canvas.StrokeSize = Math.Max(2f, size * 0.025f);
            var mx = cx + (float)Math.Cos(am) * radius * 0.78f;
            var my = cy + (float)Math.Sin(am) * radius * 0.78f;
            canvas.DrawLine(cx, cy, mx, my);

            // Second hand (SecondHandColor)
            canvas.StrokeColor = _e.SecondHandColor;
            canvas.StrokeSize  = Math.Max(1f, size * 0.015f);
            var sx = cx + (float)Math.Cos(as_) * radius * 0.9f;
            var sy = cy + (float)Math.Sin(as_) * radius * 0.9f;
            canvas.DrawLine(cx, cy, sx, sy);

            // Center cap
            canvas.FillColor = _e.TimeColor;
            canvas.FillEllipse(cx - size * 0.02f, cy - size * 0.02f, size * 0.04f, size * 0.04f);
        }
    }
}
