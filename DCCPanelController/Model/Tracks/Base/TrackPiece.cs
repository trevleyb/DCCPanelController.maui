using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackPiece : Track {

    [ObservableProperty]
    [property: EditableBool(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel", Group = "Attributes", Order = 1)]
    private bool _isHidden;

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isOccupied;

    [ObservableProperty]
    [property: EditableColor(Name = "Track Color", Description = "Color of the Track or leave None to use defaults.", Group = "Attributes", Order = 2)]
    private Color? _trackColor;

    [ObservableProperty]
    [property: EditableTrackType(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new[] { TrackStyleTypeEnum.Mainline, TrackStyleTypeEnum.Branchline }, Group = "Attributes", Order = 5)]
    private TrackStyleTypeEnum _trackTypeEnum = TrackStyleTypeEnum.Mainline;

    protected TrackPiece(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : base(parent) {
        TrackTypeEnum = styleTypeEnum;
    }

    protected TrackPiece(Panel? parent = null) : base(parent) { }

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
        var style = SvgStyles.GetStyle(TrackTypeEnum, TrackStyleImageEnum.Normal, Parent);
        if (TrackColor is not null) {
            style = new SvgStyleBuilder()
                   .AddExistingStyle(style)
                   .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(TrackColor))
                   .Build();
        }

        if (IsHidden) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttributeEnum.Hidden, Parent);
        if (IsOccupied) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttributeEnum.Occupied, Parent);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }
}