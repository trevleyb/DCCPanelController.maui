using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private TurnoutStateEnum _whenClosedOrActive;
    [ObservableProperty] private TurnoutStateEnum _whenThrownOrInActive;
}

public enum TurnoutStateEnum {
    Closed,
    Thrown,
    Unknown
}