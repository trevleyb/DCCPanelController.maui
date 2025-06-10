namespace DCCCommon.Events;

public class DccLightArgs: EventArgs {
    public required string LightId { get; init;}
    public required string UserName { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsOn { get; init;}
    public bool IsOff => !IsOn;
}