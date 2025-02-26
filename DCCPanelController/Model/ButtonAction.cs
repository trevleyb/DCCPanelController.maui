using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

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