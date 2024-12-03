using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackPieceBase : TrackBase {

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Name (ID)", Description = "Right Hand Turnout", TrackTypes = new [] { TrackStyleType.Mainline , TrackStyleType.Branchline})]
    private TrackStyleType _trackType = TrackStyleType.Mainline;

    [ObservableProperty] 
    [property: EditableBoolProperty(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel")]
    private bool _isHidden = false;

    protected override SvgImage ActiveImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var imageInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Normal, TrackRotation);

            // get the Image from the SvgImageManager
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = SvgImages.GetImage(imageInfo.imageSource);

            // Apply the various styles that need to be applied based on the 
            // details that we have within the context of this track type
            // --------------------------------------------------------------------------------------------------
            trackInfo.
            
        }
    }

    protected override SvgImage SymbolImage {
        get {
            
            if (_trackImages.SymbolImage is { ImageSource: not null } symbolImage) {
                symbolImage.ImageSource.ApplyStyle(GetTrackStyle(DefaultState));
                symbolImage.ImageSource.SetOccupied(false);
                symbolImage.ImageSource.ForceImageRefresh();
                return symbolImage.ImageSource.Image;
            }
            return _trackImages.SymbolImage.ImageSource?.Image;
            
        }
    }
}







    /// <summary>
    /// Set up the Active Image. For example, as a straight image, if it is in position 0, then it is straight.
    /// If we rotate +45, then it is the Angle image at rotation 180
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    protected void SetActiveImage() {
        IsBusy = true;
        IsRefreshing = true;
        try {
            // Get the image that should be displayed on the screen based on the direction or rotation that we
            // have and the current state. However, the rotation of the image itself might be different so we 
            // set Rotation to the value defined against the active image. 
            // -----------------------------------------------------------------------------------------------
            ActiveImage = _trackImages.Get(TrackDirection, _state);
            if (ActiveImage is { ImageSource: not null } activeImage) {
                activeImage.ImageSource.ApplyStyle(GetTrackStyle(_state.State));
                activeImage.ImageSource.SetOccupied(IsOccupied);
                activeImage.ImageSource.ForceImageRefresh();
                ImageRotation = ActiveImage?.Rotation ?? 0;
            }

            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(ImageRotation));
        } catch (Exception ex) {
            Console.WriteLine($"Should not be here: {ex.Message}");
            ImageRotation = 0;
        }

        IsRefreshing = false;
        IsBusy = false;
    }
