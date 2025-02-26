using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Services;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages;
using DCCPanelController.View.PropertyPages.Attributes;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackTurnout : Track {

    [ObservableProperty]
    [property: EditableString(Name = "DCC Address", Description = "Address or Turnout Reference", Order = 2)]
    private string _address = string.Empty;

    [ObservableProperty]
    [property: EditableActions(ActionsContext = ActionsContext.Turnout, Group = "Actions", Description = "Buttons to set when this turnout changes", Order = 11)]
    private ButtonActions _buttonActions = [];

    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty]
    [property: EditableBool(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel", Group = "Attributes", Order = 3)]
    private bool _isHidden;

    [ObservableProperty]
    [property: JsonIgnore] private bool _isOccupied;

    [ObservableProperty]
    [property: EditableColor(Name = "Track Color", Description = "Color of the Track or leave None to use defaults.", Group = "Attributes", Order = 4)]
    private Color? _trackColor;

    [ObservableProperty]
    private TrackStyleImageEnum _trackImageEnum = TrackStyleImageEnum.Normal;

    [ObservableProperty]
    [property: EditableTrackType(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new[] { TrackStyleTypeEnum.Mainline, TrackStyleTypeEnum.Branchline }, Group = "Attributes", Order = 5)]
    private TrackStyleTypeEnum _trackTypeEnum = TrackStyleTypeEnum.Mainline;

    [ObservableProperty] private Turnout? _turnout;

    [ObservableProperty]
    [property: EditableActions(ActionsContext = ActionsContext.Turnout, Group = "Actions", Description = "Turnouts to change when ths turnout changes", Order = 10)]
    private TurnoutActions _turnoutActions = [];

    [ObservableProperty]
    [property: EditableString(Name = "Turnout ID", Description = "Turnout ID", Order = 1)]
    private string _turnoutID = string.Empty;

    private TurnoutsService? _turnoutsService;

    protected TrackTurnout(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : base(parent) {
        _trackTypeEnum = styleTypeEnum;
    }

    protected TrackTurnout(Panel? parent = null) : base(parent) {
        PropertyChanged += OnPropertyChanged;
    }

    protected TurnoutsService TurnoutsService => _turnoutsService ??= MauiProgram.ServiceHelper.GetService<TurnoutsService>() ?? throw new Exception("TurnoutsService is null");

    private TurnoutStateEnum GetCurrentTurnoutState =>
        Turnout?.State ??
        TrackImageEnum switch {
            TrackStyleImageEnum.Straight  => TurnoutStateEnum.Closed,
            TrackStyleImageEnum.Diverging => TurnoutStateEnum.Thrown,
            _                             => TurnoutStateEnum.Unknown
        };

    protected abstract void ThrowTurnout(Turnout turnout, TurnoutStateEnum state); // ( Turnout turnout)

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackImageEnum, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough = false) { // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;

        // Apply the various styles that need to be applied based on the 
        // details that we have within the context of this track type
        // --------------------------------------------------------------------------------------------------
        var style = SvgStyles.GetStyle(TrackTypeEnum, TrackImageEnum, Parent);
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

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(TurnoutID) && _turnoutsService is not null) {
            if (Turnout is not null) Turnout.PropertyChanged -= TurnoutOnPropertyChanged;
            Turnout = TurnoutsService.GetTurnoutById(TurnoutID);
            if (Turnout is not null) {
                Turnout.PropertyChanged += TurnoutOnPropertyChanged;
            }
        }
    }

    public bool SetTurnoutState(TurnoutStateEnum state) {
        if (state == TurnoutStateEnum.Unknown) return false;
        TrackImageEnum = TrackImageEnum switch {
            TrackStyleImageEnum.Diverging => TrackStyleImageEnum.Straight,
            TrackStyleImageEnum.Straight  => TrackStyleImageEnum.Diverging,
            TrackStyleImageEnum.Normal    => TrackStyleImageEnum.Diverging,
            _                             => TrackStyleImageEnum.Normal
        };

        OnPropertyChanged(nameof(TrackView));
        return true;
    }

    public bool ExecTurnoutState() {
        return ExecTurnoutState(GetCurrentTurnoutState);
    }

    public bool ExecTurnoutState(TurnoutStateEnum state) {
        SetTurnoutState(state);
        if (Turnout is { } turnout) ThrowTurnout(turnout, turnout.State);
        return true;
    }

    private void TurnoutOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (sender is Turnout turnout) {
            TrackImageEnum = GetCurrentTurnoutState switch {
                TurnoutStateEnum.Unknown => TrackStyleImageEnum.Normal,
                TurnoutStateEnum.Closed  => TrackStyleImageEnum.Straight,
                TurnoutStateEnum.Thrown  => TrackStyleImageEnum.Diverging,
                _                        => TrackStyleImageEnum.Normal
            };

            OnPropertyChanged(nameof(TrackView));
        }
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }

        _clickSoundPlayer?.Play();

        if (Turnout is null) {
            TrackImageEnum = TrackImageEnum switch {
                TrackStyleImageEnum.Diverging => TrackStyleImageEnum.Straight,
                TrackStyleImageEnum.Straight  => TrackStyleImageEnum.Diverging,
                TrackStyleImageEnum.Normal    => TrackStyleImageEnum.Diverging,
                _                             => TrackStyleImageEnum.Normal
            };
        } else {
            ExecTurnoutState(Turnout.State);
            if (Parent is not null) {
                ButtonActions.ApplyButtonActionsToPanel(Parent, Turnout.State);
                TurnoutActions.ApplyTurnoutActionsToPanel(Parent, Turnout.State);
            }
        }

        OnPropertyChanged(nameof(TrackView));
    }
}