using System.Text;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract partial class TrackTile : Tile {
    protected TrackTile(TrackEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(TrackEntity.TrackType));
        VisualProperties.Add(nameof(TrackEntity.TrackAttribute));
        VisualProperties.Add(nameof(TrackEntity.TrackColor));
        VisualProperties.Add(nameof(TrackEntity.TrackBorderColor));
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTile(string trackName, int trackRotation) {
        var svgImage = SvgImages.GetImage(trackName, trackRotation);
        var style = SetDefaultStyles();
        svgImage.ApplyStyle(style.Build());
            
        var image = new Image {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Scale = 1.5
        };
        
        image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
        image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
        return image;
    }

    /// <summary>
    /// Most Track types follow the same principles, which is that the track is either
    /// a MainLine or BranchLine and is either Hidden (in a tunnel) or Normal. 
    /// </summary>
    /// <returns>A style for a standard piece of Track</returns>
    protected SvgStyleBuilder SetDefaultStyles() {
        var style = new SvgStyleBuilder();

        if (Entity is TrackEntity trackEntity) {
            switch (trackEntity.TrackType) {
            case TrackTypeEnum.BranchLine:
                style.Add(e => e.WithName(SvgElementType.Border).Hidden())
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.BranchLineColor ?? Colors.Gray).Visible())
                     .Add(e => e.WithName(SvgElementType.Occupied).Hidden());
                break;

            case TrackTypeEnum.MainLine:
            default: 
                style.Add(e => e.WithName(SvgElementType.Border).WithColor(trackEntity.TrackBorderColor ?? Entity.Parent?.BorderColor ?? Colors.Black).Visible())
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.MainLineColor ?? Colors.Black).Visible())
                     .Add(e => e.WithName(SvgElementType.Occupied).Hidden());
                break;
            }

            switch (trackEntity.TrackAttribute) {
            case TrackAttributeEnum.Opaque:
                style.Add(e => e.WithName(SvgElementType.Dashline).Hidden());
                style.Add(e => e.WithName(SvgElementType.Tracks).WithOpacity("0.50"));
                break; 

            case TrackAttributeEnum.Dashed:
                style.Add(e => e.WithName(SvgElementType.Dashline).WithColor(Entity.Parent?.HiddenColor ?? Colors.White).Visible());
                style.Add(e => e.WithName(SvgElementType.Tracks).WithOpacity("1.0"));
                break; 
                
            case TrackAttributeEnum.Normal:
            default:
                style.Add(e => e.WithName(SvgElementType.Dashline).Hidden());
                style.Add(e => e.WithName(SvgElementType.Tracks).WithOpacity("1.0"));
                break;
            }
        }
        return style;
    } 
}