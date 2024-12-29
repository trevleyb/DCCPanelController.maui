using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class TurnoutAction : ObservableObject {
        [ObservableProperty] private Guid _id;
        [ObservableProperty] private TurnoutStateEnum _whenClosed;
        [ObservableProperty] private TurnoutStateEnum _whenThrown;
}

//[JsonConverter(typeof(JsonStringEnumConverter<TurnoutStateEnum>))]
public enum TurnoutStateEnum { 
        Closed,
        Thrown,
        Unknown
}