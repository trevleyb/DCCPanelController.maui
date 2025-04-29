using DCCClients.Common;

namespace DCCClients.Events;

public class DccSignalArgs(string signalId, SignalAspectEnum aspect) : EventArgs {
    public string SignalId { get; } = signalId;
    public SignalAspectEnum Aspect { get; } = aspect;
}
