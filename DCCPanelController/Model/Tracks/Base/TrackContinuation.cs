using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackContinuationBase : TrackBase {
    [ObservableProperty] 
    [property: EditableBoolProperty(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel")]
    private bool _isHidden;

    [ObservableProperty] private bool _isOccupied;

    [ObservableProperty] 
    [property: EditableTrackImageProperty(Name = "Track Style", Description = "Style of this track piece", TrackTypes = new[] { TrackStyleImage.Arrow, TrackStyleImage.Lines })]
    private TrackStyleImage _trackImage = TrackStyleImage.Arrow;

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new[] { TrackStyleType.Mainline, TrackStyleType.Branchline })]
    private TrackStyleType _trackType = TrackStyleType.Mainline;

    [JsonIgnore]
    protected override SvgImage ActiveImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackImage, TrackRotation);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            ImageRotation = trackInfo.ImageRotation;
            TrackRotation = trackInfo.TrackRotation;

            // Apply the various styles that need to be applied based on the 
            // details that we have within the context of this track type
            // --------------------------------------------------------------------------------------------------
            var style = SvgStyles.GetStyle(TrackType, TrackImage, Parent?.Defaults);
            if (IsHidden) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttribute.Hidden,Parent?.Defaults);
            if (IsOccupied) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttribute.Occupied,Parent?.Defaults);
            return imageInfo.ApplyStyle(style);
        }
    }

    [JsonIgnore]
    protected override SvgImage SymbolImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Symbol, 0);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            var style = SvgStyles.GetStyle(TrackType, TrackStyleImage.Normal, Parent?.Defaults);
            return imageInfo.ApplyStyle(style);
        }
    }
    
    public override object Clone() {
        var clone = (TrackContinuationBase)MemberwiseClone();
        return clone;
    }

}