using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Attributes;
using DCCPanelController.ViewModel;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackButtonBase : TrackBase {

    [ObservableProperty]
    [property: EditableActions(ActionsContext = ActionsContext.Button, Group = "Actions", Description = "Buttons to set when this turnout changes", Order = 10)]
    private ButtonActions _buttonActions = [];

    [ObservableProperty]
    [property: EditableString(Name = "Button ID", Description = "Unique Identifier for this Button", Order = 1)]
    private string _buttonID = "";

    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty]
    [property: EditableActions(ActionsContext = ActionsContext.Button, Group = "Actions", Description = "Turnouts to change when ths turnout changes", Order = 11)]
    private TurnoutActions _turnoutActions = [];

    public ButtonStateEnum State = ButtonStateEnum.Unknown;
    protected TrackStyleImageEnum TrackImageEnum = TrackStyleImageEnum.Normal;

    protected TrackButtonBase(Panel? parent = null) : base(parent) {
        PropertyChanged += OnPropertyChanged;
    }

    public ButtonStateEnum ToggleButtonState =>
        State switch {
            ButtonStateEnum.Active   => ButtonStateEnum.Inactive,
            ButtonStateEnum.Inactive => ButtonStateEnum.Active,
            _                        => ButtonStateEnum.Active
        };

    public bool SetButtonState(ButtonStateEnum state) {
        if (state == ButtonStateEnum.Unknown) return false;
        State = state;
        OnPropertyChanged(nameof(TrackView));
        return true;
    }

    public bool ExecButtonState() {
        return ExecButtonState(State);
    }

    public bool ExecButtonState(ButtonStateEnum state) {
        SetButtonState(state);
        if (Parent is not null) {
            ButtonActions.ApplyButtonActionsToPanel(Parent, state);
            TurnoutActions.ApplyTurnoutActionsToPanel(Parent, state);
        }

        return true;
    }

    public override void CleanUp() {
        for (var i = ButtonActions.Count - 1; i >= 0; i--) {
            var action = ButtonActions[i];
            if (string.IsNullOrWhiteSpace(action.Id)) {
                ButtonActions.RemoveAt(i);
            }
        }

        for (var i = TurnoutActions.Count - 1; i >= 0; i--) {
            var action = TurnoutActions[i];
            if (string.IsNullOrWhiteSpace(action.Id)) {
                TurnoutActions.RemoveAt(i);
            }
        }
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result);
        }

        _clickSoundPlayer?.Play();
        State = ToggleButtonState;
        ExecButtonState(State);
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
        TrackImageEnum = State switch {
            ButtonStateEnum.Unknown  => TrackStyleImageEnum.Normal,
            ButtonStateEnum.Active   => TrackStyleImageEnum.Active,
            ButtonStateEnum.Inactive => TrackStyleImageEnum.InActive,
            _                        => TrackStyleImageEnum.Normal
        };
    }
}