using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.ViewModel.Tiles;

public sealed class FastClockTile : Tile, ITileDrawable {
    public FastClockTile(FastClockEntity entity, double gridSize) : base(entity, gridSize) { }

    private Profile? _profile;
    private Grid? _root;
    private GraphicsView? _gv;

    // Interpolation so seconds tick 1-by-1 at fast-clock speed (no drifting)
    private IDispatcherTimer? _tickTimer;
    private EventHandler? _tickHandler;

    private DateTime? _displayTime;      // what we render
    private DateTime? _targetTime;       // latest profile time we saw
    private DateTime? _lastProfileFast;  // last profile fast-time (for ratio)
    private DateTime? _lastProfileReal;  // last real-time (for ratio)
    private double _ratio = 12.0;        // fast seconds / real second (initial guess)
    private const double MinRatio = 0.25, MaxRatio = 300.0;

    // Track whether we’re subscribed to the profile to avoid double-hooking
    private bool _profileSubscribed;

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not FastClockEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        // Resolve Profile: Entity -> Parent (Panel) -> Panels -> Profile
        _profile = (e.Parent as Panel)?.Panels?.Profile;

        // Seed display time from profile or real clock
        var seed = _profile?.FastClock ?? DateTime.Now;
        _displayTime = seed;
        _targetTime  = seed;
        _lastProfileFast = seed;
        _lastProfileReal = DateTime.UtcNow;

        // Container returned to Tile.SetContent
        _root = new Grid {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            BackgroundColor = Colors.Transparent
        };

        _gv = new GraphicsView {
            InputTransparent = true,
            BackgroundColor = Colors.Transparent, // keep analog truly round
            Drawable = new FastClockDrawable(e, () => _displayTime ?? seed)
        };
        // Bind typical visuals to entity (like DrawableCircleTile)
        _gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        _gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));

        _root.Children.Add(_gv);

        // Listen to entity changes (colors/type/etc.) and our own design-mode changes
        e.PropertyChanged += OnEntityPropertyChanged;
        this.PropertyChanged += OnSelfPropertyChanged;

        // Hook profile time & start ticking only if NOT in design mode
        if (!IsDesignMode &&_profile?.FastClockState == FastClockStateEnum.On) {
            SubscribeProfile();
            EnsureTickerRunning();
        }

        return _root;
    }

    protected override void Cleanup() {
        base.Cleanup();
        if (Entity is FastClockEntity e) e.PropertyChanged -= OnEntityPropertyChanged;
        this.PropertyChanged -= OnSelfPropertyChanged;

        UnsubscribeProfile();
        StopTicker();

        _gv = null;
        _root = null;
        _profile = null;
    }

    // ---------------------------
    // Event handlers
    // ---------------------------

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (_gv is null) return;

        // If the type flips Analog <-> Digital, drawable reads the enum each draw. Just invalidate.
        if (e.PropertyName == nameof(FastClockEntity.FastclockType)) {
            _gv.Invalidate();
            return;
        }

        // Any visual change: redraw
        _gv.Invalidate();
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(IsDesignMode) || e.PropertyName == nameof(_profile.FastClockState)) {
            HandleDesignModeChanged();
        }
    }

    private void HandleDesignModeChanged() {
        if (IsDesignMode && _profile?.FastClockState == FastClockStateEnum.On ) {
            // Freeze: snapshot to the latest profile time, then stop updates
            if (_profile is not null) {
                var snap = _profile.FastClock;
                _displayTime = snap;
                _targetTime  = snap;
                _lastProfileFast = snap;
                _lastProfileReal = DateTime.UtcNow;
            }
            StopTicker();
            UnsubscribeProfile();
            _gv?.Invalidate(); // show static face
        } else {
            // Resume live updates
            if (_profile is not null) {
                var snap = _profile.FastClock;
                _displayTime = snap;
                _targetTime  = snap;
                _lastProfileFast = snap;
                _lastProfileReal = DateTime.UtcNow;
            }
            SubscribeProfile();
            EnsureTickerRunning();
            _gv?.Invalidate();
        }
    }

    // ---------------------------
    // Profile subscription & timer
    // ---------------------------

    private void SubscribeProfile() {
        if (_profile is null || _profileSubscribed) return;
        _profile.PropertyChanged += OnProfilePropertyChanged;
        _profileSubscribed = true;
    }

    private void UnsubscribeProfile() {
        if (_profile is null || !_profileSubscribed) return;
        _profile.PropertyChanged -= OnProfilePropertyChanged;
        _profileSubscribed = false;
    }

    private void OnProfilePropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDesignMode) return; // guard: ignore updates in design mode
        if (e.PropertyName != nameof(Profile.FastClock)) return; // only care about fast clock

        var nowReal = DateTime.UtcNow;
        var nowFast = _profile!.FastClock; // authoritative time

        // Update target and estimate fast ratio
        _targetTime = nowFast;

        if (_lastProfileFast is { } lastFast && _lastProfileReal is { } lastReal) {
            var dFast = (nowFast - lastFast).TotalSeconds;
            var dReal = (nowReal - lastReal).TotalSeconds;
            if (dReal > 0.005) {
                var est = dFast / dReal;
                if (!double.IsNaN(est) && !double.IsInfinity(est)) {
                    _ratio = Math.Clamp(est, MinRatio, MaxRatio);
                    UpdateTimerIntervalFromRatio();
                }
            }
        }

        _lastProfileFast = nowFast;
        _lastProfileReal = nowReal;

        // If we’re far behind, snap to stay in sync
        if (_displayTime is { } disp && _targetTime is { } tgt) {
            var gap = Math.Abs((tgt - disp).TotalSeconds);
            if (gap >= 15) _displayTime = tgt;
        }

        _gv?.Invalidate();
    }

    private void EnsureTickerRunning() {
        if (IsDesignMode) return; // don’t run timer in design mode

        if (_tickTimer != null) {
            if (!_tickTimer.IsRunning) _tickTimer.Start();
            return;
        }

        _tickTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_tickTimer is null) return;

        UpdateTimerIntervalFromRatio();

        _tickHandler = (_, __) => {
            if (_gv is null || IsDesignMode) return;

            // Step exactly +1 second per tick (so 1,2,3... at fast speed)
            if (_displayTime is null) _displayTime = _targetTime ?? DateTime.Now;
            else _displayTime = _displayTime.Value.AddSeconds(1);

            // Gentle resync if we lag far behind latest profile time
            if (_targetTime is { } tgt && _displayTime is { } disp) {
                var ahead = (disp - tgt).TotalSeconds;
                if (ahead < -30) _displayTime = tgt;
            }

            _gv.Invalidate();
        };

        _tickTimer.Tick += _tickHandler;
        _tickTimer.Start();
    }

    private void StopTicker() {
        if (_tickTimer is null) return;
        if (_tickHandler is not null) _tickTimer.Tick -= _tickHandler;
        _tickTimer.Stop();
        _tickTimer = null;
        _tickHandler = null;
    }

    private void UpdateTimerIntervalFromRatio() {
        if (_tickTimer is null) return;
        // seconds step period in REAL ms = 1000 / ratio
        var ms = 1000.0 / Math.Max(_ratio, MinRatio);
        var clamped = Math.Clamp(ms, 16.0, 5000.0); // allow very slow or very fast fast-clocks
        _tickTimer.Interval = TimeSpan.FromMilliseconds(clamped);
    }

    // ---------------------------
    // Drawable
    // ---------------------------

    private sealed class FastClockDrawable : IDrawable {
        private readonly FastClockEntity _e;
        private readonly Func<DateTime>  _time; // interpolated display time

        public FastClockDrawable(FastClockEntity e, Func<DateTime> timeProvider) {
            _e = e; _time = timeProvider;
        }

        public void Draw(ICanvas canvas, RectF dirty) {
            canvas.SaveState();
            canvas.Antialias = true;

            if (_e.FastclockType == FastClockTypeEnum.Digital) DrawDigital(canvas, dirty);
            else DrawAnalog(canvas, dirty);

            canvas.RestoreState();
        }

        // DIGITAL: Even 8-cell layout "HH:mm:ss"
        private void DrawDigital(ICanvas canvas, RectF area) {
            // Rect background + border
            canvas.FillColor = _e.BackgroundColor;
            canvas.FillRectangle(area);

            var border = _e.BorderWidth; //Math.Max(1f, Math.Min(area.Width, area.Height) * 0.04f);
            canvas.StrokeColor = _e.BorderColor;
            canvas.StrokeSize  = border;
            var inset = area.Inflate(new SizeF(-border / 2f));
            canvas.DrawRectangle(inset);

            var pad   = Math.Max(inset.Width, inset.Height) * 0.08f;
            var inner = inset.Inflate(new SizeF(-pad, -pad));

            var t = _time();
            var text = t.ToString("HH:mm:ss"); // 8 glyphs

            var cols  = 8f;
            var cellW = inner.Width / cols;
            var cellH = inner.Height;

            var fontSize = (float)Math.Min(cellW * 0.9f, cellH * 0.85f);

            for (int i = 0; i < text.Length && i < 8; i++) {
                var ch = text[i].ToString();
                var x  = inner.Left + i * cellW;
                var rect = new RectF((float)x, inner.Top, (float)cellW, (float)cellH);

                // last two glyphs are 's','s' (positions 6,7)
                canvas.FontColor = i switch {
                    0 or 1 => _e.TimeColor,
                    2 or 5 => _e.TicksColor,
                    3 or 4 => _e.TimeColor,
                    6 or 7 => _e.SecondHandColor,
                    _      => _e.TimeColor
                };
                canvas.FontSize  = fontSize;
                canvas.DrawString(ch, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        // ANALOG: transparent tile; circular face + ring; 12 ticks only (12/3/6/9 emphasized)
        private void DrawAnalog(ICanvas canvas, RectF area) {
            var size   = Math.Min(area.Width, area.Height);
            var cx     = area.Left + area.Width / 2f;
            var cy     = area.Top  + area.Height / 2f;
            var radius = size * 0.48f;

            // Face (dial)
            var faceRect = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            canvas.FillColor = _e.BackgroundColor;
            canvas.FillEllipse(faceRect);

            // Ring
            var ringStroke = _e.BorderWidth;
            canvas.StrokeColor = _e.BorderColor;
            canvas.StrokeSize  = ringStroke;
            var ring = faceRect.Inflate(new SizeF(-ringStroke / 2f));
            canvas.DrawEllipse(ring);

            // 12 hour ticks; emphasize 12/3/6/9
            for (int i = 0; i < 12; i++) {
                var strong = (i % 3 == 0);
                var tickLen    = radius * (strong ? 0.14f : 0.08f);
                var tickStroke = Math.Max(1f, size * (strong ? 0.02f : 0.012f));

                var a = (float)(i / 12.0 * 2 * Math.PI - Math.PI / 2);
                var rOuter = radius * 0.98f;
                var rInner = rOuter - tickLen;

                var x1 = cx + (float)Math.Cos(a) * rInner;
                var y1 = cy + (float)Math.Sin(a) * rInner;
                var x2 = cx + (float)Math.Cos(a) * rOuter;
                var y2 = cy + (float)Math.Sin(a) * rOuter;

                canvas.StrokeColor = _e.TicksColor;
                canvas.StrokeSize  = tickStroke;
                canvas.DrawLine(x1, y1, x2, y2);
            }

            // Hour numerals
            var fontSize = radius * 0.23f;
            canvas.FontColor = _e.TicksColor;
            canvas.FontSize  = fontSize;
            for (int h = 1; h <= 12; h++) {
                var ang = (float)((h / 12.0) * 2 * Math.PI - Math.PI / 2);
                var r   = radius * 0.74f;
                var tx  = cx + (float)(Math.Cos(ang) * r);
                var ty  = cy + (float)(Math.Sin(ang) * r);

                var w    = radius * 0.35f;
                var rect = new RectF(tx - w / 2f, ty - fontSize / 2f, w, fontSize * 1.1f);
                canvas.DrawString(h.ToString(), rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            // Hands (step on whole seconds of interpolated display time)
            var now    = _time();
            var hour   = now.Hour % 12 + now.Minute / 60.0 + Math.Floor((double)now.Second) / 3600.0;
            var minute = now.Minute + Math.Floor((double)now.Second) / 60.0;
            var second = Math.Floor((double)now.Second);

            var ah  = (float)(hour   / 12.0 * 2 * Math.PI - Math.PI / 2);
            var am  = (float)(minute / 60.0 * 2 * Math.PI - Math.PI / 2);
            var as_ = (float)(second / 60.0 * 2 * Math.PI - Math.PI / 2);

            // Hour hand
            canvas.StrokeColor = _e.TimeColor;
            canvas.StrokeSize  = Math.Max(2f, size * 0.035f);
            var hx = cx + (float)Math.Cos(ah) * radius * 0.55f;
            var hy = cy + (float)Math.Sin(ah) * radius * 0.55f;
            canvas.DrawLine(cx, cy, hx, hy);

            // Minute hand
            canvas.StrokeSize = Math.Max(2f, size * 0.025f);
            var mx = cx + (float)Math.Cos(am) * radius * 0.78f;
            var my = cy + (float)Math.Sin(am) * radius * 0.78f;
            canvas.DrawLine(cx, cy, mx, my);

            // Second hand
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
