using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Services;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackTurnoutBase : TrackBase {
    private readonly TurnoutsService? _turnoutsService;

    [ObservableProperty] 
    [property: EditableStringProperty(Name = "ID", Description = "Turnout ID")]
    private string _id = string.Empty;

    [ObservableProperty] 
    [property: EditableBoolProperty(Name = "Hidden Track", Description = "Indicates track hidden such as in a tunnel")]
    private bool _isHidden;

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Track Type", Description = "Track is Mainline or Branchline", TrackTypes = new[] { TrackStyleType.Mainline, TrackStyleType.Branchline })]
    private TrackStyleType _trackType = TrackStyleType.Mainline;

    [ObservableProperty] 
    [property: EditableTurnoutProperty(Name = "Actions", Group="Actions",  Description = "ID of the item to do an action against")]
    private List<TrackTurnoutAction> _actions = [];

    [JsonIgnore] [ObservableProperty] private bool _isOccupied;
    [ObservableProperty] private TrackStyleImage _trackImage = TrackStyleImage.Normal;

    private Turnout? _turnout;

    protected TrackTurnoutBase() {
        _turnoutsService = MauiProgram.ServiceHelper.GetService<TurnoutsService>();
        if (_turnoutsService is null) throw new Exception("TurnoutsService is null");
        PropertyChanged += OnPropertyChanged;
    }

    [JsonIgnore]
    protected override SvgImage ActiveImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackImage, TrackRotation);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            ImageRotation = trackInfo.ImageRotation;
            TrackRotation = trackInfo.TrackRotation;

            Console.WriteLine($"Track: {TrackImage}:{TrackRotation} = {trackInfo.ImageSource}:{trackInfo.ImageRotation}");

            // Apply the various styles that need to be applied based on the 
            // details that we have within the context of this track type
            // --------------------------------------------------------------------------------------------------
            var style = SvgStyles.GetStyle(TrackType, TrackImage);
            if (IsHidden) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttribute.Hidden);
            if (IsOccupied) style = SvgStyles.ApplyStyleAttributes(style, TrackStyleAttribute.Occupied);
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
            var style = SvgStyles.GetStyle(TrackType, TrackStyleImage.Normal);
            return imageInfo.ApplyStyle(style);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(Id) && _turnoutsService is not null) {
            _turnout = _turnoutsService.GetTurnoutById(Id);
            if (_turnout is not null) {
                _turnout.PropertyChanged += TurnoutOnPropertyChanged;
            }
        }
    }

    private void TurnoutOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (sender is Turnout turnout) {
            TrackImage = turnout.State switch {
                TurnoutStateEnum.Unknown => TrackStyleImage.Normal,
                TurnoutStateEnum.Closed  => TrackStyleImage.Straight,
                TurnoutStateEnum.Thrown  => TrackStyleImage.Diverging,
                _                        => TrackStyleImage.Normal
            };

            OnPropertyChanged(nameof(Image));
        }
    }
    
    public override object Clone() {
        var clone = (TrackTurnoutBase)MemberwiseClone();
        return clone;
    }
}

public partial class TrackTurnoutAction : ObservableObject {
    [ObservableProperty] private TurnoutStateEnum _closedState = TurnoutStateEnum.Unknown; // State to set the item to when Thrown

    [ObservableProperty] private string? _id; // ID of the item to do an action against

    [ObservableProperty] private TurnoutStateEnum _thrownState = TurnoutStateEnum.Unknown; // State to set the item to when Closed
}