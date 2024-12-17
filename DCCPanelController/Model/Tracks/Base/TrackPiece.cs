using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackPieceBase : TrackBase {
    
    protected TrackPieceBase(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : base(parent) {
        _trackTypeEnum = styleTypeEnum;
    }
    
    protected TrackPieceBase(Panel? parent= null) : base(parent) { }
    
    [ObservableProperty] 
    [property: EditableBoolProperty(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel")]
    private bool _isHidden;

    [ObservableProperty] [property: JsonIgnore]
    private bool _isOccupied;

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new[] { TrackStyleTypeEnum.Mainline, TrackStyleTypeEnum.Branchline })]
    private TrackStyleTypeEnum _trackTypeEnum = TrackStyleTypeEnum.Mainline;
    
    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
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
        var style = SvgStyles.GetStyle(TrackTypeEnum, TrackStyleImageEnum.Normal, Parent?.Defaults);
        if (IsHidden) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttributeEnum.Hidden,Parent?.Defaults);
        if (IsOccupied) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttributeEnum.Occupied,Parent?.Defaults);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }
}