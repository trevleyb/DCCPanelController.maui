using System.Security.Cryptography.X509Certificates;
using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCCommon.Events;

namespace DCCCommon.Client;

public interface IDccClient {
    Task<IResult> ConnectAsync();
    Task<IResult> DisconnectAsync();
    Task<IResult> ForceRefreshAsync(string? type = null);
    
    Task<IResult> SendCmdAsync(string message);
    Task<IResult> SendTurnoutCmdAsync(string turnout, bool thrown);
    Task<IResult> SendRouteCmdAsync(string route, bool active);
    Task<IResult> SendSignalCmdAsync(string signal, SignalAspectEnum aspect);

    Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync();
    Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync();
    Task<IResult> ValidateConnectionAsync();

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