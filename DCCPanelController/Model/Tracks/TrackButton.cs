using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Actions;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages;
using DCCPanelController.View.PropertyPages.Attributes;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks;

public partial class TrackButton(Panel? parent = null) : Track(parent), ITrackButton, ITrackSymbol, ITrackID {
    [ObservableProperty] [property: EditableActions(ActionsContext = ActionsContext.Button, Group = "Actions", Description = "Buttons to set when this turnout changes", Order = 10)]
    private ButtonActions _buttonActions = [];

    [ObservableProperty]
    [property: EditableString(Name = "Button ID", Description = "Unique Identifier for this Button", Order = 1)]
    private string _iD = "";

    [property: EditableInt(Name = "Layer", Group = "Attributes", Description = "What Layer does this peice sit on?", MinValue = 1, MaxValue = 5, Order = 5)]
    public new int Layer {
        get => base.Layer;
        set => base.Layer = value;
    }
    
    [ObservableProperty] [property: EditableBool(Name = "IsEnabled", Description = "Is this button active and Enabled?", Order = 2)]
    private bool _isEnabled = true;

    [ObservableProperty]
    public ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: EditableActions(ActionsContext = ActionsContext.Button, Group = "Actions", Description = "Turnouts to change when ths turnout changes", Order = 11)]
    private TurnoutActions _turnoutActions = [];

    protected TrackStyleImageEnum TrackImageEnum = TrackStyleImageEnum.Normal;

    public TrackButton() : this(null) {
        PropertyChanged += OnPropertyChanged;
    }

    public string Name => "Button";

    public void Clicked() {
        ClickSound(ClickSoundTypeEnum.Button);
        
        // Toggle the state if the button has been pressed
        // --------------------------------------------------------
        var state = State switch {
            ButtonStateEnum.Active   => ButtonStateEnum.Inactive,
            ButtonStateEnum.Inactive => ButtonStateEnum.Active,
            _                        => ButtonStateEnum.Active
        };

        ExecButtonState(state);
        OnPropertyChanged(nameof(TrackView));
    }

    // Set the color/or state of the button but no action performed
    // -------------------------------------------------------------
    public bool SetButtonState(ButtonStateEnum state) {
        if (state == ButtonStateEnum.Unknown) return false;
        State = state;
        OnPropertyChanged(nameof(TrackView));
        return true;
    }

    public bool ExecButtonState(ButtonStateEnum state, ActionList actioned) {
        SetButtonState(state);

        if (Parent is not null) {
            ActionApplyButton.ApplyButtonActions(Parent, this, actioned);
        }

        return true;
    }

    public ITrack Clone(Panel parent) {
        var cloned = Clone<TrackButton>(parent);
        cloned.ID = cloned.Parent?.NextButtonID() ?? "";
        Debug.Assert(cloned != null, nameof(cloned) + " != null");
        return cloned;
    }

    public bool ExecButtonState(ButtonStateEnum state) {
        // When calling execute from a click, pass an empty collection 
        // This collection is populated to track what buttons and turnouts we have 
        // processed, so we don't do one more than once. 
        return ExecButtonState(state, new ActionList());
    }

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool? passthrough) {
        var image = CreateImageView(TrackImageEnum, TrackRotation, gridSize, passthrough ?? Passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough ?? Passthrough);
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
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Button, TrackImageEnum, Parent);
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

    protected override void Setup() {
        Layer = 4;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "ButtonCorner", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Active, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Active, "ButtonCorner", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.InActive, "Button", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.InActive, "ButtonCorner", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}