using DCCClients;
using DCCClients.Events;
using DCCPanelController.Models;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Services;

public sealed class ConnectionService {

    private IDccClient? _dccClient;
    private readonly Profile _profile;
    private ConnectionInfo? _connectionInfo;

    public event EventHandler<ConnectionChangedEvent>? ConnectionChanged;
    
    public ConnectionService(Profile profile) {
        _profile = profile;
        _connectionInfo = profile.ActiveConnectionInfo;
    }

    public async Task<IDccClient?> Connect() => await Connect(_profile.ActiveConnectionInfo);
    public async Task<IDccClient?> Connect(ConnectionInfo? connectionInfo) {
        if (connectionInfo is not null) _connectionInfo = connectionInfo;
        ArgumentNullException.ThrowIfNull(_connectionInfo);
        _dccClient = await GetClient(_connectionInfo);
        OnConnectionChanged();
        return _dccClient;
    }
    
    public void Disconnect() {
        if (_dccClient is not null) {
            _dccClient.Disconnect();
            _dccClient.TurnoutMsgReceived -= DccClientOnTurnoutMsgReceived;
            _dccClient.RouteMsgReceived -= DccClientOnRouteMsgReceived;
        }
        _dccClient = null;
        OnConnectionChanged();
    }

    public bool IsConnected => _dccClient is not null && _dccClient.IsConnected;

    private async Task<IDccClient> GetClient(ConnectionInfo? connection = null) {
        
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (_dccClient is { IsConnected: true }) return _dccClient;
        if (_dccClient is { IsConnected: false }) Disconnect();
        
        // If we don't have a connection setup, and we did not provide settings, then we error
        // --------------------------------------------------------------------------
        if (connection?.Settings is null) throw new ArgumentNullException(nameof(connection),"No Connection Details provided.");
        
        // Attempt to connect to the Service provided and return the client
        // --------------------------------------------------------------------------
        _dccClient = DccClientFactory.Create(connection.Settings);
        var result = await _dccClient.ConnectAsync();
        if (result.IsFailure) _dccClient = new DccInvalidClient(connection.Settings);
        
        _dccClient.TurnoutMsgReceived += DccClientOnTurnoutMsgReceived;
        _dccClient.RouteMsgReceived += DccClientOnRouteMsgReceived;
        return _dccClient;  
    }

    /// <summary>
    /// Global Update of any routes that get sent by the server
    /// </summary>
    private void DccClientOnRouteMsgReceived(object? sender, DccRouteArgs e) {
        Route? route = null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Id == e.RouteId) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Id == e.DccAddress) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Name == e.RouteId) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Name == e.DccAddress) ?? null;
        if (route is not null) {
            Console.WriteLine($"Route {e.RouteId} updated");
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
        } else {
            Console.WriteLine($"Route {e.RouteId} added");
            _profile.Routes.Add(new Route() {
                Id = e.RouteId,
                State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive
            });
        }
    }

    /// <summary>
    /// Global update of any Turnouts that appear once we do a connection. WiThrottle in particular only reports
    /// any defined turnouts on connection. 
    /// </summary>
    private void DccClientOnTurnoutMsgReceived(object? sender, DccTurnoutArgs e) {
        Turnout? turnout = null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.Id == e.TurnoutId) ?? null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.Id == e.DccAddress) ?? null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.Name == e.TurnoutId) ?? null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.Name == e.DccAddress) ?? null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.DccAddress == e.TurnoutId) ?? null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.DccAddress == e.DccAddress) ?? null;
        if (turnout is not null) {
            Console.WriteLine($"Turnout {e.TurnoutId} updated");
            turnout.State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown;
        } else if (_profile is not null && _profile.Turnouts is not null) {
            Console.WriteLine($"Turnout {e.TurnoutId} added");
            _profile.Turnouts.Add(new Turnout() {
                Name = e.TurnoutId,
                Id   = e.TurnoutId,
                DccAddress = e.DccAddress,
                State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown
            });
        }
    }

    private void OnConnectionChanged() {
        ConnectionChanged?.Invoke(this, new ConnectionChangedEvent() { IsConnected = IsConnected});
    }
}