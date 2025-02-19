using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;


public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private ButtonStateEnum _whenActiveOrClosed = ButtonStateEnum.Unknown;
    [ObservableProperty] private ButtonStateEnum _whenInactiveOrThrown = ButtonStateEnum.Unknown;
    [ObservableProperty] private bool _cascade = false;
}

public enum ButtonStateEnum {
    Inactive,
    Active,
    Toggle, 
    Leave,
    Unknown
}