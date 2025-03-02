using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Actions;

namespace DCCPanelController.Model;

public class TurnoutActions : ObservableCollection<TurnoutAction> { }

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