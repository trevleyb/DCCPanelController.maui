using System.ComponentModel;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract class TrackTile : Tile, ITileTrack {
    private const float HighlightColorAlpha = 0.25f;
    public SvgImage? SvgImage { get; protected set; }
    
    protected TrackTile(TrackEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TrackEntity.Rotation));
        VisualProperties.Add(nameof(TrackEntity.TrackType));
        VisualProperties.Add(nameof(TrackEntity.TrackAttribute));
        VisualProperties.Add(nameof(TrackEntity.TrackColor));
        VisualProperties.Add(nameof(TrackEntity.TrackBorderColor));
        VisualProperties.Add(nameof(IsOccupied));
        VisualProperties.Add(nameof(IsPath));
        if (Entity is TrackEntity { Occupancy: {} occupancy }) {
            if (occupancy.Id == entity?.Occupancy?.Id) IsOccupied = occupancy.IsOccupied;
            occupancy.PropertyChanged += (sender, args) => {
                if (occupancy.Id == entity?.Occupancy?.Id) {
                    IsOccupied = occupancy.IsOccupied;
                }
            };
        }
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        return CreateTrackTileAsCanvas(trackName, trackRotation, DefaultScaleFactor, null);
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, float scale) {
        return CreateTrackTileAsCanvas(trackName, trackRotation, scale, null);
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation, SvgStyle addStyle) {
        return CreateTrackTileAsCanvas(trackName, trackRotation, DefaultScaleFactor, addStyle);
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas(string trackName, int trackRotation, float scale, SvgStyle? addStyle = null) {
        SvgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        if (addStyle is not null) SvgImage.ApplyStyle(addStyle);
        SvgImage.ApplyStyle(style.Build());

        var canvas = SvgImage.AsCanvas(SvgImage.Rotation, 1);
        canvas.HorizontalOptions = LayoutOptions.Fill;
        canvas.VerticalOptions = LayoutOptions.Fill;
        canvas.SetBinding(OpacityProperty, new Binding(nameof(Opacity), BindingMode.OneWay, source: Entity));
        canvas.SetBinding(ZIndexProperty, new Binding(nameof(TrackEntity.Layer), BindingMode.TwoWay, source: Entity));

        var absoluteLayout = new AbsoluteLayout();
        AbsoluteLayout.SetLayoutBounds(canvas, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
        absoluteLayout.Children.Add(canvas);
        return absoluteLayout;
    }

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
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.BranchLineColor ?? Colors.Gray).Visible());
                break;

            case TrackTypeEnum.MainLine:
            default:
                style.Add(e => e.WithName(SvgElementType.Border).WithColor(trackEntity.TrackBorderColor ?? Entity.Parent?.MainlineBorderColor ?? Colors.Black).Visible())
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