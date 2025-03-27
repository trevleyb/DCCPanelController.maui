using DCCClients.Common;
using DCCClients.Events;
using DCCClients.Interfaces;
using DCCClients.WiThrottle.Client;

namespace DCCClients;

public interface IDccClient {
    Task<IResult> ConnectAsync();
    Task<IResult> ReconnectAsync();
    IResult Disconnect();
    
    IResult SendCmd(string message);
    IResult SendTurnoutCmd(string dccAddress, bool thrown);
    IResult SendRouteCmd(string dccAddress, bool active);
    IResult SendSignalCmd(string dccAddress, SignalAspectEnum aspect);
    
    event EventHandler<DccErrorArgs>     ConnectionError;
    event EventHandler<DccMessageArgs>   MessageReceived;
    event EventHandler<DccTurnoutArgs>   TurnoutMsgReceived;
    event EventHandler<DccRouteArgs>     RouteMsgReceived;
    event EventHandler<DccOccupancyArgs> OccupancyMsgReceived;
}