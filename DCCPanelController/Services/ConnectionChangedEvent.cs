namespace DCCPanelController.Services;

public class ConnectionChangedEvent : EventArgs {
    public bool IsConnected { get; set; }
}