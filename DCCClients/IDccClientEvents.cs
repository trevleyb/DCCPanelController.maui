using DCCClients.Common;
using DCCClients.Events;

namespace DCCClients;

public interface IDccClientEvents {
    event EventHandler<DccErrorArgs> ConnectionError;
    event EventHandler<DccMessageArgs> MessageReceived;
    event EventHandler<DccTurnoutArgs> TurnoutMsgReceived;
    event EventHandler<DccRouteArgs> RouteMsgReceived;
    event EventHandler<DccOccupancyArgs> OccupancyMsgReceived;
    event EventHandler<DccSignalArgs> SignalMsgReceived;
}
