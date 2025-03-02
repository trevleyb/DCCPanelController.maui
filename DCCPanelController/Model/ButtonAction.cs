using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Actions;

namespace DCCPanelController.Model;

public class ButtonActions : ObservableCollection<ButtonAction> {}

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private bool _cascade;
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenActiveOn = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenInactiveOff = ButtonStateEnum.Unknown;
}

public enum ButtonStateEnum {
    Inactive,
    Active,
    Unknown
}