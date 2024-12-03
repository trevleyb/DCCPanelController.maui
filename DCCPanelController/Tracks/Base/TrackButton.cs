using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackButtonBase : TrackBase {

    [ObservableProperty] 
    [property: EditableTrackTypeProperty(Name = "Name (ID)", Description = "Right Hand Turnout", TrackTypes = new [] { TrackStyleType.Button})]
    private TrackStyleType _type = TrackStyleType.Button;

}