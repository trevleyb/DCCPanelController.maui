using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TrackTile : Tile {
    private const float HighlightColorAlpha = 0.25f;

    protected TrackTile(TrackEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TrackEntity.Rotation));
        VisualProperties.Add(nameof(TrackEntity.TrackType));
        VisualProperties.Add(nameof(TrackEntity.TrackAttribute));
        VisualProperties.Add(nameof(TrackEntity.TrackColor));
        VisualProperties.Add(nameof(TrackEntity.TrackBorderColor));
    }

    public Color HighlightColor {
        get {
            if (IsPath) return Colors.CornflowerBlue.WithAlpha(HighlightColorAlpha);
            if (IsOccupied) return Colors.Tomato.WithAlpha(HighlightColorAlpha);
            return Colors.Transparent;
        }
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, float scale = DefaultScaleFactor) {
        return CreateTrackTileAsCanvas(trackName, trackRotation, scale);
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas(string trackName, int trackRotation, float scale = DefaultScaleFactor) {
        var svgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        svgImage.ApplyStyle(style.Build());

        var canvas = svgImage.AsCanvas(svgImage.Rotation, 1);
        canvas.HorizontalOptions = LayoutOptions.Fill;
        canvas.VerticalOptions = LayoutOptions.Fill;
        canvas.SetBinding(OpacityProperty, new Binding(nameof(Opacity), BindingMode.OneWay, source: Entity));
        canvas.SetBinding(BackgroundColorProperty, new Binding(nameof(HighlightColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: this));
        canvas.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), BindingMode.TwoWay, source: Entity));

        var absoluteLayout = new AbsoluteLayout();
        AbsoluteLayout.SetLayoutBounds(canvas, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
        absoluteLayout.Children.Add(canvas);
        return absoluteLayout;
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsImage(string trackName, int trackRotation, float scale = DefaultScaleFactor) {
        var svgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        svgImage.ApplyStyle(style.Build());

        var image = svgImage.AsImage(svgImage.Rotation, scale);
        image.SetBinding(OpacityProperty, new Binding(nameof(Opacity), BindingMode.OneWay, source: Entity));
        image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
        image.SetBinding(BackgroundColorProperty, new Binding(nameof(HighlightColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: this));
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
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.BranchLineColor ?? Colors.Gray).Visible());
                break;

            case TrackTypeEnum.MainLine:
            default:
                style.Add(e => e.WithName(SvgElementType.Border).WithColor(trackEntity.TrackBorderColor ?? Entity.Parent?.BorderColor ?? Colors.Black).Visible())
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.MainLineColor ?? Colors.Black).Visible());
                break;
            }

            switch (trackEntity.TrackAttribute) {
            case TrackAttributeEnum.Dashed:
                style.Add(e => e.WithName(SvgElementType.Dashline).WithColor(Entity.Parent?.HiddenColor ?? Colors.White).Visible());
                break;

            case TrackAttributeEnum.Normal:
            default:
                style.Add(e => e.WithName(SvgElementType.Dashline).Hidden());
                break;
            }
        }
        return style;
    }
    
    // @formatter:off
    public bool IsOccupied {get; set => SetField(ref field, value); }
    public bool IsPath {get; set => SetField(ref field, value); }
    // @formatter:on
}