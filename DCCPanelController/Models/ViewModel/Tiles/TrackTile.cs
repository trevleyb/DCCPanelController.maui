using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TrackTile : Tile, ITileTrack {
    private const float HighlightColorAlpha = 0.25f;

    protected TrackTile(TrackEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TrackEntity.Rotation));
        VisualProperties.Add(nameof(TrackEntity.TrackType));
        VisualProperties.Add(nameof(TrackEntity.TrackAttribute));
        VisualProperties.Add(nameof(TrackEntity.TrackColor));
        VisualProperties.Add(nameof(TrackEntity.TrackBorderColor));
        VisualProperties.Add(nameof(IsOccupied));
        VisualProperties.Add(nameof(IsPath));
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

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) => CreateTrackTileAsCanvas(trackName, trackRotation, DefaultScaleFactor);

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, float scale) => CreateTrackTileAsCanvas(trackName, trackRotation, scale);

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, SvgStyle addStyle) => CreateTrackTileAsCanvas(trackName, trackRotation, DefaultScaleFactor, addStyle);

    // TrackTile.cs
    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas(
        string trackName, int trackRotation, float scale, SvgStyle? addStyle = null)
    {
        Console.WriteLine($"Creating track tile {trackName} {trackRotation} {EntityConnections.ConvertDirectionsToString(((TrackEntity)Entity).GetCurrentConnections)}");
        
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
        canvasView.PaintSurface += (sender, e) =>
        {
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
        canvasView.VerticalOptions   = LayoutOptions.Fill;
        canvasView.SetBinding(OpacityProperty,  new Binding(nameof(Opacity),      source: Entity));
        canvasView.SetBinding(ZIndexProperty,   new Binding(nameof(TrackEntity.Layer), source: Entity));

        var absoluteLayout = new AbsoluteLayout();
        AbsoluteLayout.SetLayoutBounds(canvasView, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
        absoluteLayout.Children.Add(canvasView);
        return absoluteLayout;
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
    
    // protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas(string trackName, int trackRotation, float scale, SvgStyle? addStyle = null) {
    //     SvgImage = SvgImages.GetImage(trackName, trackRotation);
    //     var style = GetDefaultStyle();
    //     if (addStyle is { }) style.AddExisting(addStyle); //SvgImage.ApplyStyle(addStyle);
    //     SvgImage.ApplyStyle(style.Build());
    //
    //     var canvas = SvgImage.AsCanvas(SvgImage.Rotation, 1);
    //     canvas.HorizontalOptions = LayoutOptions.Fill;
    //     canvas.VerticalOptions = LayoutOptions.Fill;
    //     canvas.SetBinding(OpacityProperty, new Binding(nameof(Opacity), BindingMode.OneWay, source: Entity));
    //     canvas.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), BindingMode.TwoWay, source: Entity));
    //
    //     if (ShowPointsOverlay) {
    //         
    //     }
    //     
    //     var absoluteLayout = new AbsoluteLayout();
    //     AbsoluteLayout.SetLayoutBounds(canvas, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
    //     absoluteLayout.Children.Add(canvas);
    //     return absoluteLayout;
    // }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsImage(string trackName, int trackRotation, float scale = DefaultScaleFactor) {
        SvgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        SvgImage.ApplyStyle(style.Build());

        var image = SvgImage.AsImage(SvgImage.Rotation, scale);
        image.SetBinding(OpacityProperty, new Binding(nameof(Opacity), BindingMode.OneWay, source: Entity));
        image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: SvgImage));
        return image;
    }

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
                style.Add(e => e.WithName(SvgElementType.Terminator).WithColor(straightEntity.TrackColor ?? Entity.Parent?.TerminatorColor ?? Colors.Gray).Visible());
            }
            
            if (IsPath && DisplayMode == TileDisplayMode.Normal) {
                var color = Entity.Parent?.ShowPathColor ?? Colors.CornflowerBlue.WithAlpha(HighlightColorAlpha);
                style.Add(e => e.WithName(SvgElementType.Track).WithColor(color).Visible());
            } else if (IsOccupied && DisplayMode == TileDisplayMode.Normal) {
                var color = Entity.Parent?.OccupiedColor ?? Colors.Tomato.WithAlpha(HighlightColorAlpha);
                style.Add(e => e.WithName(SvgElementType.Track).WithColor(color).Visible());
            }
        }
        return style;
    }
    
    // @formatter:off
    public bool IsOccupied {get; set => SetField(ref field, value); }
    public bool IsPath {get; set => SetField(ref field, value); }
    // @formatter:on
}