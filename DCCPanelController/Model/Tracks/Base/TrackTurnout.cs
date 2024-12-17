using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Services;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackTurnoutBase : TrackBase {

    protected TrackTurnoutBase(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : base(parent) { 
        _trackTypeEnum = styleTypeEnum;
    }

    protected TrackTurnoutBase(Panel? parent= null) : base(parent) { 
        PropertyChanged += OnPropertyChanged;
    }

    private TurnoutsService? _turnoutsService;

    [ObservableProperty] 
    [property: EditableStringProperty(Name = "TurnoutID", Description = "Turnout ID")]
    private string _turnoutId = string.Empty;

    [ObservableProperty] 
    [property: EditableBoolProperty(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel")]
    private bool _isHidden;

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new[] { TrackStyleTypeEnum.Mainline, TrackStyleTypeEnum.Branchline })]
    private TrackStyleTypeEnum _trackTypeEnum = TrackStyleTypeEnum.Mainline;

    [ObservableProperty] 
    [property: EditableTurnoutProperty(Name = "Actions", Group="Actions",  Description = "ID of the item to do an action against")]
    private List<TrackTurnoutAction> _actions = [];

    [ObservableProperty] [property:JsonIgnore] private bool _isOccupied;
    [ObservableProperty] private TrackStyleImageEnum _trackImageEnum = TrackStyleImageEnum.Normal;

    private Turnout? _turnout;
    protected abstract void ThrowTurnout(Turnout turnout, TurnoutStateEnum state); // ( Turnout turnout)
   
    protected TurnoutsService TurnoutsService => _turnoutsService ??= MauiProgram.ServiceHelper.GetService<TurnoutsService>() ?? throw new Exception("TurnoutsService is null");
    
    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackImageEnum, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough = false) {        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;

        // Apply the various styles that need to be applied based on the 
        // details that we have within the context of this track type
        // --------------------------------------------------------------------------------------------------
        var style = SvgStyles.GetStyle(TrackTypeEnum, TrackImageEnum, Parent?.Defaults);
        if (IsHidden) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttributeEnum.Hidden,Parent?.Defaults);
        if (IsOccupied) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttributeEnum.Occupied,Parent?.Defaults);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(TurnoutId) && _turnoutsService is not null) {
            _turnout = TurnoutsService.GetTurnoutById(TurnoutId);
            if (_turnout is not null) {
                _turnout.PropertyChanged += TurnoutOnPropertyChanged;
            }
        }
    }

    private void TurnoutOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (sender is Turnout turnout) {
            TrackImageEnum = turnout.State switch {
                TurnoutStateEnum.Unknown => TrackStyleImageEnum.Normal,
                TurnoutStateEnum.Closed  => TrackStyleImageEnum.Straight,
                TurnoutStateEnum.Thrown  => TrackStyleImageEnum.Diverging,
                _                        => TrackStyleImageEnum.Normal
            };

            OnPropertyChanged(nameof(TrackView));
        }
    }
    
    private IAudioPlayer? _clickSoundPlayer;
    public void Clicked() {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result);
        }
        _clickSoundPlayer?.Play();

        if (_turnout is null) {
            TrackImageEnum = TrackImageEnum switch {
                TrackStyleImageEnum.Diverging => TrackStyleImageEnum.Straight,
                TrackStyleImageEnum.Straight  => TrackStyleImageEnum.Diverging,
                TrackStyleImageEnum.Normal    =>TrackStyleImageEnum.Diverging,
                _                         => TrackStyleImageEnum.Normal
            };
        } else {
            switch (_turnout.State) {
            case TurnoutStateEnum.Closed:
                TrackImageEnum = TrackStyleImageEnum.Diverging;
                ThrowTurnout(_turnout, TurnoutStateEnum.Thrown);
                break;
            case TurnoutStateEnum.Thrown or TurnoutStateEnum.Unknown:
                TrackImageEnum = TrackStyleImageEnum.Straight;
                ThrowTurnout(_turnout, TurnoutStateEnum.Closed);
                break;
            default:
                TrackImageEnum = TrackStyleImageEnum.Normal;
                ThrowTurnout(_turnout, TurnoutStateEnum.Closed);
                break;
            }
        }
        OnPropertyChanged(nameof(TrackView));
    }

}

public partial class TrackTurnoutAction : ObservableObject {
    [ObservableProperty] private TurnoutStateEnum _closedState = TurnoutStateEnum.Unknown; // State to set the item to when Thrown
    [ObservableProperty] private string? _turnoutId; // ID of the item to do an action against
    [ObservableProperty] private TurnoutStateEnum _thrownState = TurnoutStateEnum.Unknown; // State to set the item to when Closed
}