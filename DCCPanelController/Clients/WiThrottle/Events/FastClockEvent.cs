using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Clients.WiThrottle.Events;

public class FastClockEvent(DateTime time, int state) : IClientEvent {
    public DateTime Time { get; init; } = time;

    public FastClockStateEnum State { get; set; } = state switch {
        0 => FastClockStateEnum.Off,
        1 => FastClockStateEnum.On,
        _ => FastClockStateEnum.Unknown
    };
        
    public override string ToString() {
        return $"FASTCLOCK: {Time.ToShortDateString()}";
    }
}