using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private TurnoutStateEnum _whenClosedStraight;
    [ObservableProperty] private TurnoutStateEnum _whenThrownDiverging;
}

public enum TurnoutStateEnum {
    Closed,
    Thrown,
    Unknown
}