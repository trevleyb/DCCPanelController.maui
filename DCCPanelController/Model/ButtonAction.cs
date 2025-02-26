using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;


public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenActiveOn = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenInactiveOff = ButtonStateEnum.Unknown;
    [ObservableProperty] private bool _cascade = false;
}

public enum ButtonStateEnum {
    Inactive,
    Active,
    Unknown
}