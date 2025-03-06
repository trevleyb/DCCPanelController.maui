using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.DataModel.Helpers;
using DCCPanelController.Model.DataModel.Interfaces;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackButton : Track, ITrackID {
    public override string Name => "Button";
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private Actions<ButtonStateEnum> _buttonActions = [];
    [ObservableProperty] private Actions<TurnoutStateEnum> _turnoutActions = [];
    [ObservableProperty] private ButtonStateEnum _buttonState  = ButtonStateEnum.Unknown;
    
    public TrackButton() { }
    public TrackButton(TrackButton track) : base(track) {
        Id = TrackID.NextButtonID(Parent?.NamedButtons ??[]);
        ButtonActions = new Actions<ButtonStateEnum>(track.ButtonActions);
        TurnoutActions = new Actions<TurnoutStateEnum>(track.TurnoutActions);
        ButtonState = ButtonStateEnum.Unknown;
    }
   
}