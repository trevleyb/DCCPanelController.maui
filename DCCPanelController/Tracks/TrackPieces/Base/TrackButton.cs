using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.TrackPieces.Base;

public abstract partial class TrackButtonBase : TrackBase {
    [ObservableProperty] private bool? _buttonState;
    [ObservableProperty] private TrackStyleImage _trackImage = TrackStyleImage.Normal;

    protected TrackButtonBase() {
        PropertyChanged += OnPropertyChanged;
    }

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
            var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackImage);
            return imageInfo.ApplyStyle(style);
        }
    }

    protected override SvgImage SymbolImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Symbol, 0);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackStyleImage.Normal);
            return imageInfo.ApplyStyle(style);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        // Add code to determine if the state of the button has changed
        TrackImage = ButtonState switch {
            true  => TrackStyleImage.Active,
            false => TrackStyleImage.InActive,
            _     => TrackStyleImage.Normal
        };
    }
}