using DCCClients.Events;

namespace DCCClients;

public abstract class DccClient {
    public event EventHandler<DccErrorArgs>? ConnectionError;
    public event EventHandler<DccMessageArgs>? MessageReceived;
    public event EventHandler<DccTurnoutArgs>? TurnoutMsgReceived;
    public event EventHandler<DccRouteArgs>? RouteMsgReceived;
    public event EventHandler<DccOccupancyArgs>? OccupancyMsgReceived;

    protected virtual void OnConnectionError(DccErrorArgs e) {
        ConnectionError?.Invoke(this, e);
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
}