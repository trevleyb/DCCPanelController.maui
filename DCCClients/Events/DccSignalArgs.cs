using DCCClients.Common;

namespace DCCClients.Events;

public class DccSignalArgs(string dccAddress, string signalId, SignalAspectEnum aspect) : EventArgs {
    public string DccAddress { get; } = dccAddress;
    public string SignalId { get; } = signalId;
    public SignalAspectEnum Aspect { get; } = aspect;
}
