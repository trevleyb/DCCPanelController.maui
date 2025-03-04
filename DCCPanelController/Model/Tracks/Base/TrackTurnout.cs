using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Actions;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.Helpers;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages;
using DCCPanelController.View.PropertyPages.Attributes;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackTurnout : Track, ITrackTurnout, ITrackID {
    [ObservableProperty]
    [property: EditableString(Name = "DCC Address", Description = "Address or Turnout Reference", Order = 2)]
    private string _address = string.Empty;

    [ObservableProperty]
    [property: EditableActions(ActionsContext = ActionsContext.Turnout, Group = "Actions", Description = "Buttons to set when this turnout changes", Order = 11)]
    private ButtonActions _buttonActions = [];

    private IAudioPlayer? _clickSoundPlayer;

    [ObservableProperty]
    [property: EditableString(Name = "Turnout ID", Description = "Turnout ID", Order = 1)]
    private string _iD = string.Empty;

    [ObservableProperty]
    [property: EditableBool(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel", Group = "Attributes", Order = 3)]
    private bool _isHidden;

    [ObservableProperty]
    [property: JsonIgnore] private bool _isOccupied;

    [ObservableProperty]
    private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;

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

    private TurnoutsService? _turnoutsService;

    protected TrackTurnout(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : this(parent) {
        _trackTypeEnum = styleTypeEnum;
    }

    protected TrackTurnout(Panel? parent = null) : base(parent) {
        PropertyChanged += OnPropertyChanged;
    }

    protected TurnoutsService TurnoutsService => _turnoutsService ??= MauiProgram.ServiceHelper.GetService<TurnoutsService>() ?? throw new Exception("TurnoutsService is null");

    public abstract string Name { get; }
    public abstract ITrack Clone(Panel parent);

    public bool SetTurnoutState(TurnoutStateEnum state) {
        if (state == TurnoutStateEnum.Unknown) return false;
        State = state;
        if (Turnout is { } turnout) ThrowTurnout(turnout, state);
        OnPropertyChanged(nameof(TrackView));
        return true;
    }

    public bool ExecTurnoutState(TurnoutStateEnum state, ActionList actioned) {
        SetTurnoutState(state);

        if (Parent is not null) {
            ActionApplyTurnout.ApplyTurnoutActions(Parent, this, actioned);
        }

        return true;
    }

    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }
        _clickSoundPlayer?.Play();

        TrackPointsValidator.ClearTrackPaths(this?.Parent?.Tracks);
        var state = State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };

        ExecTurnoutState(state);
        OnPropertyChanged(nameof(TrackView));
    }

    protected abstract void ThrowTurnout(Turnout turnout, TurnoutStateEnum state); // ( Turnout turnout)

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool? passthrough) {
        var image = CreateImageView(TrackImageEnum, TrackRotation, gridSize, passthrough ?? Passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough ?? Passthrough);
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

        style = SvgStyles.ApplyHiddenStyle(style, Parent, IsHidden);
        style = SvgStyles.ApplyOccupiedOrPathStyle(style,Parent,IsOccupied,IsPath);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        TrackImageEnum = State switch {
            TurnoutStateEnum.Unknown => TrackStyleImageEnum.Normal,
            TurnoutStateEnum.Closed  => TrackStyleImageEnum.Straight,
            TurnoutStateEnum.Thrown  => TrackStyleImageEnum.Diverging,
            _                        => TrackStyleImageEnum.Normal
        };

        if (e.PropertyName is nameof(ID) && _turnoutsService is not null) {
            // ???
            //Turnout = TurnoutsService.GetTurnoutById(ID);
        }
    }

    public bool ExecTurnoutState(TurnoutStateEnum state) {
        // When calling execute from a click, pass an empty collection 
        // This collection is populated to track what buttons and turnouts we have 
        // processed, so we don't do one more than once. 
        return ExecTurnoutState(state, new ActionList());
    }
}