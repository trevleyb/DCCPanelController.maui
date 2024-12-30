using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackButtonBase : TrackBase {

    private IAudioPlayer? _clickSoundPlayer;
    protected ButtonStateEnum ButtonState = ButtonStateEnum.Unknown;
    protected TrackStyleImageEnum TrackImageEnum = TrackStyleImageEnum.Normal;

    [ObservableProperty] 
    [property: EditableTurnoutProperty(Name = "Turnout Actions", Group="Actions",  Description = "Turnouts to change when ths turnout changes")]
    private TurnoutActions _turnoutActions = [];

    [ObservableProperty] 
    [property: EditableTurnoutProperty(Name = "Button Actions", Group="Actions",  Description = "Buttons to set when this turnout changes")]
    private ButtonActions _buttonActions = [];
    
    protected TrackButtonBase(Panel? parent = null) : base(parent) {
        PropertyChanged += OnPropertyChanged;
    }

    public void SetButtonState(ButtonStateEnum state) {
        ButtonState = state;
        OnPropertyChanged(nameof(TrackView));
    }

    public void ExecButtonState(ButtonStateEnum state) {
        SetButtonState(state);
        if (Parent is not null) {
            ButtonActions.ApplyButtonActionsToPanel(Parent,state);
            TurnoutActions.ApplyTurnoutActionsToPanel(Parent,state);
        }
    }

    public ButtonStateEnum ToggleButtonState => 
         ButtonState switch {
            ButtonStateEnum.Active   => ButtonStateEnum.InActive,
            ButtonStateEnum.InActive => ButtonStateEnum.Active,
            _                        => ButtonStateEnum.Active
        };
    
    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result);
        }
        _clickSoundPlayer?.Play();
        ButtonState = ToggleButtonState;
        ExecButtonState(ButtonState);
        OnPropertyChanged(nameof(TrackView));
    }
    
    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackImageEnum, TrackRotation, gridSize, passthrough);
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
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Button, TrackImageEnum, Parent?.Defaults);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        // Add code to determine if the state of the button has changed
        TrackImageEnum = ButtonState switch {
            ButtonStateEnum.Unknown  => TrackStyleImageEnum.Normal,
            ButtonStateEnum.Active   => TrackStyleImageEnum.Active,
            ButtonStateEnum.InActive => TrackStyleImageEnum.InActive,
            _                        => TrackStyleImageEnum.Normal
        };
    }
}