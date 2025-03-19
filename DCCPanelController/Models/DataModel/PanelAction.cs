using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

public partial class ButtonActions : ObservableCollection<ButtonAction> {
    public ButtonActions() { }
    public ButtonActions(ButtonActions buttonActions) {
        foreach (var action in buttonActions) Add(new ButtonAction(action));
    }
}

public partial class TurnoutActions : ObservableCollection<TurnoutAction> {
    public TurnoutActions() { }
    public TurnoutActions(TurnoutActions buttonActions) {
        foreach (var action in buttonActions) Add(new TurnoutAction(action));
    }
}

public partial class RouteActions : ObservableCollection<RouteAction> {
    public RouteActions() { }
    public RouteActions(RouteActions buttonActions) {
        foreach (var action in buttonActions) Add(new RouteAction(action));
    }
}

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenOn = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenOff = ButtonStateEnum.Unknown;
    [ObservableProperty] private bool _cascade = false;

    public ButtonAction() { }
    public ButtonAction(ButtonAction action) {
        Id = action.Id;
        WhenOn = action.WhenOn;
        WhenOff = action.WhenOff;
        Cascade = action.Cascade;
    }
}

public partial class TurnoutAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private TurnoutStateEnum  _whenClosed = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _whenThrown = TurnoutStateEnum.Unknown;
    [ObservableProperty] private bool _cascade = false;

    public TurnoutAction() { }
    public TurnoutAction(TurnoutAction action) {
        Id = action.Id;
        WhenClosed = action.WhenClosed;
        WhenThrown = action.WhenThrown;
        Cascade = action.Cascade;
    }
}

public partial class RouteAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private RouteStateEnum _whenActive = RouteStateEnum.Unknown;
    [ObservableProperty] private RouteStateEnum _whenInactive = RouteStateEnum.Unknown;
    [ObservableProperty] private bool _cascade = false;

    public RouteAction() { }
    public RouteAction(RouteAction action) {
        Id = action.Id;
        WhenActive = action.WhenActive;
        WhenInactive = action.WhenInactive;
        Cascade = action.Cascade;
    }
}
