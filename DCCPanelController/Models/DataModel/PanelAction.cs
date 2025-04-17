using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

public class ButtonActions : ObservableCollection<ButtonAction> {
    public ButtonActions() { }

    public ButtonActions(ButtonActions buttonActions) {
        foreach (var action in buttonActions) Add(new ButtonAction(action));
    }
}

public class TurnoutActions : ObservableCollection<TurnoutAction> {
    public TurnoutActions() { }

    public TurnoutActions(TurnoutActions buttonActions) {
        foreach (var action in buttonActions) Add(new TurnoutAction(action));
    }
}

public class RouteActions : ObservableCollection<RouteAction> {
    public RouteActions() { }

    public RouteActions(RouteActions buttonActions) {
        foreach (var action in buttonActions) Add(new RouteAction(action));
    }
}

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenOff = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenOn = ButtonStateEnum.Unknown;

    public ButtonAction() { }

    public ButtonAction(ButtonAction action) {
        Id = action.Id;
        WhenOn = action.WhenOn;
        WhenOff = action.WhenOff;
        Cascade = action.Cascade;
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

public partial class RouteAction : ObservableObject {
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private RouteStateEnum _whenActive = RouteStateEnum.Unknown;
    [ObservableProperty] private RouteStateEnum _whenInactive = RouteStateEnum.Unknown;

    public RouteAction() { }

    public RouteAction(RouteAction action) {
        Id = action.Id;
        WhenActive = action.WhenActive;
        WhenInactive = action.WhenInactive;
        Cascade = action.Cascade;
    }
}