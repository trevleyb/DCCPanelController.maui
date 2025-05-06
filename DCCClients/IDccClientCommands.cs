using DCCClients.Common;
using DCCClients.Events;

namespace DCCClients;

public interface IDccClientCommands {
    bool IsConnected { get; }

    Task<IResult> SendCmdAsync(string message);
    Task<IResult> SendTurnoutCmdAsync(string dccAddress, bool thrown);
    Task<IResult> SendRouteCmdAsync(string dccAddress, bool active);
    Task<IResult> SendSignalCmdAsync(string dccAddress, SignalAspectEnum aspect);

    void ForceRefresh(string? type = "all");
    
}
