using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackButtonBase : TrackBase {

    private IAudioPlayer? _clickSoundPlayer;

    protected TrackButtonBase(Panel? parent = null) : base(parent) {
        PropertyChanged += OnPropertyChanged;
    }
   
    [ObservableProperty] 
    private bool? _buttonState;
    
    [ObservableProperty] 
    private TrackStyleImage _trackImage = TrackStyleImage.Normal;

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
        OnPropertyChanged(nameof(ActiveImage));
    }

    protected abstract void PushButtonAction(bool isActive); // ( Turnout turnout)

    
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
            var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackImage, Parent?.Defaults);
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
            var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackStyleImage.Normal, Parent?.Defaults);
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