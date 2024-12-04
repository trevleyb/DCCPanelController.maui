using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackContinuationBase : TrackBase {

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new [] { TrackStyleType.Mainline , TrackStyleType.Branchline})]
    private TrackStyleType _trackType = TrackStyleType.Mainline;

    [ObservableProperty] [property: EditableTrackImageProperty(Name = "Track Style", Description = "Style of this track piece", TrackTypes = new[] { TrackStyleImage.Arrow, TrackStyleImage.Lines })]
    private TrackStyleImage _trackImage = TrackStyleImage.Arrow;

    [ObservableProperty] 
    [property: EditableBoolProperty(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel")]
    private bool _isHidden = false;

    [ObservableProperty] private bool _isOccupied = false;
    
    protected override SvgImage ActiveImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackImage, TrackDirection);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            TrackRotation = trackInfo.Rotation;
            
            Console.WriteLine($"Track: {TrackImage}:{TrackDirection} = {trackInfo.ImageSource}:{trackInfo.Rotation}");
            
            // Apply the various styles that need to be applied based on the 
            // details that we have within the context of this track type
            // --------------------------------------------------------------------------------------------------
            var style = SvgStyles.GetStyle(TrackType, TrackImage);
            if (IsHidden) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttribute.Hidden);
            if (IsOccupied) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttribute.Occupied);
            return imageInfo.ApplyStyle(style);
        }
    }

    protected override SvgImage SymbolImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Symbol, 0);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            var style = SvgStyles.GetStyle(TrackType, TrackStyleImage.Normal);
            return imageInfo.ApplyStyle(style);
        }
    }
}