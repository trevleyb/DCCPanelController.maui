using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.StyleManager;
using DCCPanelController.View.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract partial class TrackTile : Tile {
    private const double DefaultScale = 1.5;
    
    // @formatter:off
    public bool IsOccupied {get; set => SetField(ref field, value); }
    public bool IsPath {get; set => SetField(ref field, value); }
    // @formatter:on

    protected TrackTile(TrackEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TrackEntity.TrackType));
        VisualProperties.Add(nameof(TrackEntity.TrackAttribute));
        VisualProperties.Add(nameof(TrackEntity.TrackColor));
        VisualProperties.Add(nameof(TrackEntity.TrackBorderColor));
        VisualProperties.Add(nameof(TrackEntity.IsOpaque));
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        return CreateTrackTileAsCanvas(trackName, trackRotation);
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas(string trackName, int trackRotation) {
        var svgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        svgImage.ApplyStyle(style.Build());

        var canvas = svgImage.AsCanvas(TileWidth*DefaultScale,TileHeight*DefaultScale,svgImage.Rotation);
        canvas.HorizontalOptions = LayoutOptions.Fill;
        canvas.VerticalOptions = LayoutOptions.Fill;
        
        if (Entity is TrackEntity { IsOpaque : true } entity) canvas.Opacity = entity?.Parent?.OpacityAttribute ?? 1.0;
        if (IsPath) canvas.BackgroundColor = Colors.CornflowerBlue;
        if (IsOccupied) canvas.BackgroundColor = Colors.Tomato;
        return canvas;
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsImage(string trackName, int trackRotation) {
        var svgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = GetDefaultStyle();
        svgImage.ApplyStyle(style.Build());
        
        var image = svgImage.AsImage(1.5f);
        if (Entity is TrackEntity { IsOpaque : true } entity) image.Opacity = entity?.Parent?.OpacityAttribute ?? 1.0;
        if (IsPath) image.BackgroundColor = Colors.CornflowerBlue;
        if (IsOccupied) image.BackgroundColor = Colors.Tomato;
        image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
        return image;
    }

    /// <summary>
    /// Most Track types follow the same principles, which is that the track is either
    /// a MainLine or BranchLine and is either Hidden (in a tunnel) or Normal. 
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
}