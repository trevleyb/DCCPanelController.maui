using DCCCommon.Events;

namespace DCCCommon.Client;

public abstract class DccClientBase {
    public event EventHandler<DccStateChangedArgs>? ConnectionStateChanged;
    public event EventHandler<DccMessageArgs>? MessageReceived;
    public event EventHandler<DccTurnoutArgs>? TurnoutMsgReceived;
    public event EventHandler<DccRouteArgs>? RouteMsgReceived;
    public event EventHandler<DccOccupancyArgs>? OccupancyMsgReceived;
    public event EventHandler<DccSignalArgs>? SignalMsgReceived;
    public event EventHandler<DccSensorArgs>? SensorMsgReceived;
    public event EventHandler<DccLightArgs>? LightMsgReceived;
    
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
    
    protected virtual void OnSensorMsgReceived(DccSensorArgs e) {
        SensorMsgReceived?.Invoke(this, e);
    }

    protected virtual void OnLightMsgReceived(DccLightArgs e) {
        LightMsgReceived?.Invoke(this, e);
    }

}