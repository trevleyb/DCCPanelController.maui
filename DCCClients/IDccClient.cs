using DCCClients.Common;
using DCCClients.Events;

namespace DCCClients;

public interface IDccClient {
    bool IsConnected { get; }
    Task<IResult> ConnectAsync();
    Task<IResult> ReconnectAsync();
    IResult Disconnect();

    IResult SendCmd(string message);
    IResult SendTurnoutCmd(string dccAddress, bool thrown);
    IResult SendRouteCmd(string dccAddress, bool active);
    IResult SendSignalCmd(string dccAddress, SignalAspectEnum aspect);

    event EventHandler<DccErrorArgs> ConnectionError;
    event EventHandler<DccMessageArgs> MessageReceived;
    event EventHandler<DccTurnoutArgs> TurnoutMsgReceived;
    event EventHandler<DccRouteArgs> RouteMsgReceived;
    event EventHandler<DccOccupancyArgs> OccupancyMsgReceived;
    event EventHandler<DccSignalArgs> SignalMsgReceived;
}
