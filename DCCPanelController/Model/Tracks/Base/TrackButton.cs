using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackButtonBase : TrackBase {

    private IAudioPlayer? _clickSoundPlayer;
    protected bool? ButtonState;
    protected TrackStyleImage TrackImage = TrackStyleImage.Normal;

    protected TrackButtonBase(Panel? parent = null) : base(parent) {
        PropertyChanged += OnPropertyChanged;
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result);
        }
        _clickSoundPlayer?.Play();

        ButtonState ??= false;
        ButtonState = !ButtonState;
        TrackImage  = (ButtonState ?? false) ? TrackStyleImage.Active : TrackStyleImage.InActive;
        PushButtonAction(ButtonState ?? false);
        OnPropertyChanged(nameof(TrackView));
    }

    protected abstract void PushButtonAction(bool isActive); // ( Turnout turnout)

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImage.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackImage, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImage trackStyle, int rotation, double gridSize, bool passthrough = false) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;

        // Apply the various styles that need to be applied based on the 
        // details that we have within the context of this track type
        // --------------------------------------------------------------------------------------------------
        var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackImage, Parent?.Defaults);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
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