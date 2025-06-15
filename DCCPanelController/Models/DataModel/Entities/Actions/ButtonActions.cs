using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Entities.Actions;

public class ButtonActions : ObservableCollection<ButtonAction> {
    public ButtonActions() { }

    public ButtonActions(ButtonActions buttonActions) {
        foreach (var action in buttonActions) Add(new ButtonAction(action));
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

