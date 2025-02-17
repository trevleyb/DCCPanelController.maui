using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private TurnoutStateEnum _whenClosedOrActive;
    [ObservableProperty] private TurnoutStateEnum _whenThrownOrInActive;
    [ObservableProperty] private bool _cascade = false;
}

public enum TurnoutStateEnum {
    Closed,
    Thrown,
    Toggle, 
    AsIs,
    Unknown
}