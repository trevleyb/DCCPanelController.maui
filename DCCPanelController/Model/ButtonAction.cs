using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private ButtonStateEnum _whenOn;
    [ObservableProperty] private ButtonStateEnum _whenOff;
}

public enum ButtonStateEnum {
    InActive = 0,
    Active = 1,
    Unknown = 2
}