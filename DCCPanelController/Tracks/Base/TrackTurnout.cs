using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Model;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackTurnout : TrackPiece {

    [ObservableProperty]
    [property: EditableTurnoutProperty(Name = "Actions", Description = "ID of the item to do an action against", Group="Actions")]
    private List<TrackTurnoutAction> _actions = [];
}

public partial class TrackTurnoutAction : ObservableObject {

    [ObservableProperty]
    private string? _id; // ID of the item to do an action against

    [ObservableProperty] 
    private TurnoutStateEnum _closedState = TurnoutStateEnum.Unknown; // State to set the item to when Thrown

    [ObservableProperty]
    private TurnoutStateEnum _thrownState = TurnoutStateEnum.Unknown; // State to set the item to when Closed
}