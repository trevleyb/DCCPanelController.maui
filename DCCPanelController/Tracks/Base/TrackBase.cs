using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackBase : BaseViewModel {

    protected readonly StyleTrackImages StyleTrackImages = new();
    
    [ObservableProperty] private int _trackRotation;
    [ObservableProperty] private int _trackDirection;
    [ObservableProperty] private int _x;                    // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                    // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _width = 1;            // How Width is it (normally 1, Text might be 2)
    [ObservableProperty] private int _height = 1;           // How High is it (Normally 1, Text might be 2)
    [ObservableProperty] private int _layer = 1;            // What layer is this on? Only 1 element per layer.
    [ObservableProperty] private bool _isOccupied = false;  // Is this element currently occupied?
    [ObservableProperty] private bool _isResizable = false; // Can this element be resized? Normally False

    /// <summary>
    /// This abstract method works out, from the StyleTrackImages which image should
    /// be used given the current rotation and type and returns it as an 
    /// </summary>
    protected abstract SvgImage ActiveImage { get; }
    protected abstract void Setup();
    public ImageSource Image => ActiveImage.Image;
    public TrackConnectionsEnum[] Connections => ActiveImage.Connections.ConnectionPointsRotated(ImageRotation) ?? new SvgCompass().ConnectionPointsRotated(ImageRotation);
    
    protected TrackBase() {
        Initialise();
        OnPropertyChanged(nameof(Image));
        PropertyChanged += OnPropertyChanged;
        IsOccupied = false;
    }

    /// <summary>
    /// Manage when properties have changed as we may need to redraw the image
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(IsOccupied):
            SetActiveImage();
            break;
        }
    }
    
    public void RotateLeft() {
        TrackDirection = Compass.ToCompass(TrackDirection).Prev(_trackImages.RotateBy).ToRotation();
        SetActiveImage();
    }

    public void RotateRight() {
        TrackDirection = Compass.ToCompass(TrackDirection).Next(_trackImages.RotateBy).ToRotation();
        SetActiveImage();
    }
    public void NextState() {
        _state.Next();
        SetActiveImage();
    }

    public void PrevState() {
        _state.Prev();
        SetActiveImage();
    }

    
    /// <summary>
    /// Used to initialise the instance of this class and setup the parameters for any derived instances
    /// </summary>
    protected void Initialise() {
        Setup();
        _state.First();
        SetActiveImage();
    }

    protected void SetTrackSymbol(string imageSource) {
        AddImageSourceAndRotation(TrackStyleImage.Symbol, imageSource, (0, 0), (0, 0), (0, 0), (0, 0));
    }
   
    public void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) =>
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);

    public void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) =>
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);


    /// <summary>
    /// Adds an Image to the collection of available Images based on a Potential State
    /// </summary>
    /// <param name="trackSubType">The SubType or State of this item (Hidden, Diverging, etc)</param>
    /// <param name="imageSource"></param>
    /// <param name="trackRotation"></param>
    /// <param name="imageRotation"></param>
    //protected void AddTrackImage(_trackStyleSubType trackSubType, string imageSource, int trackRotation, int imageRotation) {
    //    _state.AddState(trackSubType);
    //    _trackImages.Add(trackRotation, trackSubType, imageSource, imageRotation);
    //}


    //protected void SetTrackStyle(string stateName, string styleName) {
    //    SetTrackStyle(stateName, SvgStyles.GetStyle(styleName));
    //}

    //protected void SetTrackStyle(string stateName, SvgStyle style) {
    //    if (_trackStyles.ContainsKey(stateName)) {
    //        _trackStyles[stateName.ToLowerInvariant()] = style;
    //    } else {
    //        AddTrackStyle(stateName.ToLowerInvariant(), style);
    //    }
    //}

    //protected void AddTrackStyle(string stateName, string styleName) {
    //    AddTrackStyle(stateName, SvgStyles.GetStyle(styleName.ToLowerInvariant()));
    //}

    //protected void AddTrackStyle(string stateName, SvgStyle style) {
    //    _trackStyles.TryAdd(stateName.ToLowerInvariant(), style);
    //}

    //protected SvgStyle GetTrackStyle(string stateName) {
    //    var selectedState = stateName ?? DefaultState;
    //    return _trackStyles.TryGetValue(selectedState.ToLowerInvariant(), out var value) ? value : SvgStyles.DefaultStyle;
   // }
}