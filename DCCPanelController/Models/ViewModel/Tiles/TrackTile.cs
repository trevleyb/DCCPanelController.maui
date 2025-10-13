using System.Diagnostics;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.Models.ViewModel.TileCache;
using MethodTimer;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TrackTile : Tile, ITileTrack {
    protected const float HighlightColorAlpha = 0.25f;

    // @formatter:off
    public bool IsOccupied {get; set => SetField(ref field, value); }
    public bool IsPath {get; set => SetField(ref field, value); }
    // @formatter:on
    
    protected TrackTile(TrackEntity entity, double gridSize) : base(entity, gridSize) {
        Watch
           .Track(nameof(TrackEntity.TrackType), () => entity.TrackType)
           .Track(nameof(TrackEntity.TrackAttribute), () => entity.TrackAttribute)
           .Track(nameof(TrackEntity.TrackColor), () => entity.TrackColor)
           .Track(nameof(TrackEntity.TrackBorderColor), () => entity.TrackBorderColor)
           .Track(nameof(TrackEntity.HiddenColor), () => entity.HiddenColor)
           .Track(nameof(TrackEntity.Occupancy), () => entity.Occupancy)
           .Track(nameof(IsOccupied), () => IsOccupied)
           .Track(nameof(IsPath), () => IsPath);

        if (Entity is TrackEntity { Occupancy: { } occupancy }) {
            if (occupancy.Id == entity?.Occupancy?.Id) IsOccupied = occupancy.IsOccupied;
            occupancy.PropertyChanged += (sender, args) => {
                if (occupancy.Id == entity?.Occupancy?.Id) {
                    IsOccupied = occupancy.IsOccupied;
                }
            };
        }
    }

    public bool ShowPointsOverlay { get; set; }

    public SvgImage? SvgImage { get; protected set; }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) => CreateTrackTileAsCanvas(trackName, trackRotation);

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, float scale) => CreateTrackTileAsCanvas(trackName, trackRotation);

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, SvgStyle addStyle) => CreateTrackTileAsCanvas(trackName, trackRotation, addStyle);

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas(string trackName, int trackRotation, SvgStyle? addStyle = null) {
        SvgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        if (addStyle is { }) style.AddExisting(addStyle);

        // Build a stable descriptor of the style to hash (names+colors+visibility)
        // ---------------------------------------------------------------------------------
        var styleDescriptor = style.ToString();
        var styleHash = TileRenderCache.HashStyle(styleDescriptor);

        // Compute pixel size once (respect your AbsoluteLayout bounds)
        // ---------------------------------------------------------------------------------
        var pxW = (int)Math.Round(GridSize * 1.5);
        var pxH = (int)Math.Round(GridSize * 1.5);

        // Create the key for our Cache
        // ---------------------------------------------------------------------------------
        var baseKey = new TileRenderKey(
            Asset: SvgImage.Filename, // resolved asset path/name
            RotationDeg: NormalizeDeg(SvgImage.Rotation),
            PixelWidth: pxW,
            PixelHeight: pxH,
            StyleHash: styleHash,
            Flags: IsPath ? "Path" : (IsOccupied ? "Occ" : null)
        );
        var cachedImage = GetOrRender(baseKey, SvgImage, style);

        var view = new SKCanvasView();
        view.PaintSurface += (sender, e) => {
            var c = e.Surface.Canvas;
            c.Clear(SKColors.Transparent);

            EnsureImageMatchesSurfaceSize(ref cachedImage, style, baseKey, e.Info.Width, e.Info.Height, SvgImage!);
            var sampling = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None);
            var dest = SKRect.Create(e.Info.Width, e.Info.Height);
            c.DrawImage(cachedImage, dest, sampling); // rotated already
        };

        view.HorizontalOptions = LayoutOptions.Fill;
        view.VerticalOptions = LayoutOptions.Fill;
        view.SetBinding(OpacityProperty, new Binding(nameof(Opacity), source: Entity));
        view.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), source: Entity));

        var bx = -GridSize * 0.25;
        var by = -GridSize * 0.25;
        var bw =  GridSize * 1.5;
        var bh =  GridSize * 1.5;
        
        var absoluteLayout = new AbsoluteLayout();
        AbsoluteLayout.SetLayoutBounds(view, SnapToPixels(bx, by, bw, bh));
        absoluteLayout.Children.Add(view);
        return absoluteLayout;
    }

    static Rect SnapToPixels(double x, double y, double w, double h) {
        var d = DeviceDisplay.MainDisplayInfo.Density; // pixels per DIP
        double Snap(double v) => Math.Round(v * d) / d;
        return new Rect(Snap(x), Snap(y), Snap(w), Snap(h));
    }

    
    private void EnsureImageMatchesSurfaceSize(ref SKImage img, SvgStyleBuilder svgStyle, TileRenderKey baseKey, int w, int h, SvgImage svg) {
        if (img.Width == w && img.Height == h) return;

        var sizedKey = baseKey with { PixelWidth = w, PixelHeight = h };
        img = TileRenderCache.Shared.TryGet(sizedKey, out var found)
            ? found
            : GetOrRender(sizedKey, svg, svgStyle);
    }

    private static int NormalizeDeg(int deg) {
        deg %= 360;
        return deg < 0 ? deg + 360 : deg;
    }

    private SKImage GetOrRender(TileRenderKey key, SvgImage svgImage, SvgStyleBuilder style) {
        if (TileRenderCache.Shared.TryGet(key, out var cached)) return cached;

        svgImage.ApplyStyle(style.Build());
        var info = new SKImageInfo(key.PixelWidth, key.PixelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var dest = SKRect.Create(key.PixelWidth, key.PixelHeight);
        svgImage.ImageManager.Draw(canvas, destWorld: dest, rotationDegrees: key.RotationDeg);

        var img = surface.Snapshot();         // immutable SKImage
        TileRenderCache.Shared.Put(key, img); // LRU + dispose on eviction inside Put
        return img;
    }

    private void ColorPoints(SvgImage svgImage) {
        if (Entity is TrackEntity entity) {
            var directions = entity.GetCurrentConnections;

            for (var i = 0; i < 8; i++) {
                var color = directions[i] switch {
                    ConnectionType.None       => null,
                    ConnectionType.Terminator => Colors.Black,
                    ConnectionType.Straight   => Colors.Green,
                    ConnectionType.Crossing   => Colors.Chartreuse,
                    ConnectionType.Closed     => Colors.BlueViolet,
                    ConnectionType.Diverging  => Colors.Red,
                    ConnectionType.Connector  => Colors.Blue,
                    _                         => null
                };

                if (color is { }) {
                    svgImage.ApplyElementStyle(PointLabel(i), "Color", color.ToRgbaHex());
                    svgImage.ApplyElementStyle(PointLabel(i), "Opacity", "50");
                } else {
                    svgImage.ApplyElementStyle(PointLabel(i), "Opacity", "0");
                }
            }
        }
    }

    private string PointLabel(int direction) => direction switch {
        0 => "PointN",
        1 => "PointNE",
        2 => "PointE",
        3 => "PointSE",
        4 => "PointS",
        5 => "PointSW",
        6 => "PointW",
        7 => "PointNW",
        _ => "PointN",
    };

    /// <summary>
    ///     Most Track types follow the same principles, which is that the track is either
    ///     a MainLine or BranchLine and is either Hidden (in a tunnel) or Normal.
    /// </summary>
    /// <returns>A style for a standard piece of Track</returns>
    protected SvgStyleBuilder GetDefaultStyle() {
        var style = new SvgStyleBuilder();
        if (Entity is TrackEntity trackEntity) {
            switch (trackEntity.TrackType) {
                case TrackTypeEnum.BranchLine:
                    style.Add(e => e.WithName(SvgElementType.Border).Hidden())
                         .Add(e => e.WithName(SvgElementType.BorderDiverging).Hidden())
                         .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.BranchLineColor ?? Colors.Gray).Visible());
                break;

                case TrackTypeEnum.MainLine:
                default:
                    style.Add(e => e.WithName(SvgElementType.Border).WithColor(trackEntity.TrackBorderColor ?? Entity.Parent?.MainlineBorderColor ?? Colors.Black).Visible())
                         .Add(e => e.WithName(SvgElementType.BorderDiverging).WithColor(trackEntity.TrackBorderColor ?? Entity.Parent?.MainlineBorderColor ?? Colors.Black).Visible())
                         .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.MainLineColor ?? Colors.Black).Visible());
                break;
            }

            switch (trackEntity.TrackAttribute) {
                case TrackAttributeEnum.Dashed:
                    style.Add(e => e.WithName(SvgElementType.Dashline).WithColor(trackEntity.HiddenColor ?? Entity.Parent?.HiddenColor ?? Colors.White).Visible());
                break;

                case TrackAttributeEnum.Normal:
                default:
                    style.Add(e => e.WithName(SvgElementType.Dashline).Hidden());
                break;
            }

            if (Entity is StraightEntity straightEntity) {
                style.Add(e => e.WithName(SvgElementType.Terminator).WithColor(straightEntity.TerminatorColor ?? Entity.Parent?.TerminatorColor ?? Colors.Gray).Visible());
            }

            if (!IsDesignMode) {
                if (IsPath) {
                    var color = Entity.Parent?.ShowPathColor ?? Colors.CornflowerBlue.WithAlpha(HighlightColorAlpha);
                    style.Add(e => e.WithName(SvgElementType.Track).WithColor(color).Visible());
                } else if (IsOccupied) {
                    var color = Entity.Parent?.OccupiedColor ?? Colors.Tomato.WithAlpha(HighlightColorAlpha);
                    style.Add(e => e.WithName(SvgElementType.Track).WithColor(color).Visible());
                }
            }
        }
        return style;
    }

    [Obsolete("This can be removed. Very Slow")]
    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvasOld(string trackName, int trackRotation, float scale, SvgStyle? addStyle = null) {
        SvgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        if (addStyle is { }) style.AddExisting(addStyle);
        SvgImage.ApplyStyle(style.Build());

        SvgImage? overlayMgr = null;
        if (ShowPointsOverlay) {
            overlayMgr = SvgImages.GetImage("points", 0); // resource name match
            ColorPoints(overlayMgr);
        }

        var canvasView = new SKCanvasView();
        canvasView.PaintSurface += (sender, e) => {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var dest = SKRect.Create(e.Info.Width, e.Info.Height);
            var rotation = SvgImage!.Rotation; // your current rotation field

            // Draw base track
            SvgImage.ImageManager.Draw(canvas, dest, rotationDegrees: rotation);

            // Draw overlay above it (same dest keeps them aligned)
            // Tweak opacity / offset / blend mode per need:
            overlayMgr?.ImageManager.Draw(canvas, dest, rotationDegrees: 0,
                opacity: 1f,
                offsetPx: new SKPoint(0, 0),
                blend: SKBlendMode.SrcOver);
        };

        canvasView.HorizontalOptions = LayoutOptions.Fill;
        canvasView.VerticalOptions = LayoutOptions.Fill;
        canvasView.SetBinding(OpacityProperty, new Binding(nameof(Opacity), source: Entity));
        canvasView.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), source: Entity));

        var absoluteLayout = new AbsoluteLayout();
        AbsoluteLayout.SetLayoutBounds(canvasView, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
        absoluteLayout.Children.Add(canvasView);
        return absoluteLayout;
    }
     
}