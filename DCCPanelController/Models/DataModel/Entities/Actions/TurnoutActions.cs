using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class TurnoutActions : ObservableCollection<TurnoutAction> {
    public TurnoutActions() { }

    public TurnoutActions(TurnoutActions buttonActions) {
        foreach (var action in buttonActions) Add(new TurnoutAction(action));
    }
}

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private TurnoutStateEnum _whenClosed = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _whenThrown = TurnoutStateEnum.Unknown;

    public TurnoutAction() { }

    public TurnoutAction(TurnoutAction action) {
        Id = action.Id;
        WhenClosed = action.WhenClosed;
        WhenThrown = action.WhenThrown;
        Cascade = action.Cascade;
    }
}
