using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Microsoft.Maui.Controls.Shapes;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackDrawLine(Panel? parent = null) : TrackDraw(parent), ITrackSymbol, ITrack {

    public string Name => "Line";

    [property: EditableInt(Name = "Width", Description = "Width of the Rectangle", Group = "Attributes")]
    public new int Width {
        get => base.Width;
        set => base.Width = value;
    }

    [property: EditableInt(Name = "Height", Description = "Height of the Rectangle", Group = "Attributes")]
    public new int Height {
        get => base.Height;
        set => base.Height = value;
    }

    [ObservableProperty] [property: EditableColor(Name = "Line Color", Description = "Line Color", Group = "Colors")]
    private Color _lineColor = Colors.Black;

    [ObservableProperty] [property: EditableInt(Name = "Line Width", Description = "Line Width", Group = "Colors")]
    private int _lineWidth = 3;
    
    public TrackDrawLine() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackDrawLine>(parent);
    }

    protected override void Setup() {
        Layer = 1;
        RotationIncrement = 0;
        Width = 1;
        Height = 1;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Line", (0, 0), (0, 0), (0, 0), (0, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Line", (0, 0), (0, 0), (0, 0), (0, 0));
    }
    
    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough = false) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;

        // Apply the various styles that need to be applied based on the 
        // details that we have within the context of this track type
        // --------------------------------------------------------------------------------------------------
        ActiveImage = imageInfo;
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var line = new Line() {
            X1 = 0,
            Y1 = 0,
            X2 = gridSize * Width,
            Y2 = gridSize * Height,
            Stroke = LineColor ?? Colors.Black,
            StrokeThickness = LineWidth,
            WidthRequest = gridSize * Width,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            ZIndex = Layer,
            Opacity = Opacity,
            InputTransparent = passthrough,
        };
        return line;        
    }
}