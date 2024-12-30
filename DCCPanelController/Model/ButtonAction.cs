using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class ButtonAction : ObservableObject {
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private ButtonStateEnum _whenActiveOrClosed;
    [ObservableProperty] private ButtonStateEnum _whenInactiveOrThrown;
}

public enum ButtonStateEnum {
    InActive = 0,
    Active = 1,
    Unknown = 2
}