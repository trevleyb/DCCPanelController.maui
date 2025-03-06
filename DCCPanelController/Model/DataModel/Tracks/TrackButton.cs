using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackButton : Track {
    public override string Name => "Button";
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private Actions<ButtonStateEnum> _buttonActions = [];
    [ObservableProperty] private Actions<TurnoutStateEnum> _turnoutActions = [];
    [ObservableProperty] private ButtonStateEnum _buttonState  = ButtonStateEnum.Unknown;
}