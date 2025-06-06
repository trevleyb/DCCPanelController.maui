using DCCCommon.Common;
using DCCCommon.Events;

namespace DCCCommon.Client;

public interface IDccClient {
    Task<IResult> ConnectAsync();
    Task<IResult> DisconnectAsync();
    Task<IResult> ReconnectAsync();
    Task<IResult> ForceRefreshAsync(string? type = null);
    Task<IResult> TestConnectionAsync();
    
    Task<IResult> SendCmdAsync(string message);
    Task<IResult> SendTurnoutCmdAsync(string dccAddress, bool thrown);
    Task<IResult> SendRouteCmdAsync(string dccAddress, bool active);
    Task<IResult> SendSignalCmdAsync(string dccAddress, SignalAspectEnum aspect);

    DccClientType Type { get; }
    bool IsConnected { get; }
    bool IsDisconnected => !IsConnected;
    
    event EventHandler<DccStateChangedArgs> ConnectionStateChanged;
    event EventHandler<DccMessageArgs> MessageReceived;
    event EventHandler<DccTurnoutArgs> TurnoutMsgReceived;
    event EventHandler<DccRouteArgs> RouteMsgReceived;
    event EventHandler<DccOccupancyArgs> OccupancyMsgReceived;
    event EventHandler<DccSignalArgs> SignalMsgReceived;

}