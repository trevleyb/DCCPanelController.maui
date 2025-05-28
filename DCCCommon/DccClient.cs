using DCCCommon.Common;
using DCCCommon.Events;

namespace DCCCommon;

public abstract class DccClient {
    
    public event EventHandler<DccStateChangedArgs>? ConnectionStateChanged;
    public event EventHandler<DccMessageArgs>? MessageReceived;
    public event EventHandler<DccTurnoutArgs>? TurnoutMsgReceived;
    public event EventHandler<DccRouteArgs>? RouteMsgReceived;
    public event EventHandler<DccOccupancyArgs>? OccupancyMsgReceived;
    public event EventHandler<DccSignalArgs>? SignalMsgReceived;

    protected virtual void OnConnectionStateChanged(DccStateChangedArgs e) {
        ConnectionStateChanged?.Invoke(this, e);
    }

    protected virtual void OnMessageReceived(DccMessageArgs e) {
        MessageReceived?.Invoke(this, e);
    }

    protected virtual void OnTurnoutMsgReceived(DccTurnoutArgs e) {
        TurnoutMsgReceived?.Invoke(this, e);
    }

    protected virtual void OnRouteMsgReceived(DccRouteArgs e) {
        RouteMsgReceived?.Invoke(this, e);
    }

    protected virtual void OnOccupancyMsgReceived(DccOccupancyArgs e) {
        OccupancyMsgReceived?.Invoke(this, e);
    }
    
    protected virtual void OnSignalMsgReceived(DccSignalArgs e) {
        SignalMsgReceived?.Invoke(this, e);
    }
}
