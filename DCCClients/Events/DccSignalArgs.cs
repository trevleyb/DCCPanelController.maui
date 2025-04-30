using DCCClients.Common;

namespace DCCClients.Events;

public class DccSignalArgs(string signalId, string aspect) : EventArgs {
    public string SignalId { get; } = signalId;
    public string Aspect { get; } = aspect;
}
