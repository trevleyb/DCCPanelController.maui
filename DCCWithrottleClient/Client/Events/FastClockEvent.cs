namespace DCCWithrottleClient.Client.Events;

public class FastClockEvent (DateTime time) : IClientEvent {
    public DateTime Time { get; init; } = time;
    public override string ToString() => $"FASTCLOCK: {Time.ToShortDateString()}";
}