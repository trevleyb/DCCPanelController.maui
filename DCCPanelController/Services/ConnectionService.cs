using DCCClients;
using DCCClients.Events;
using DCCPanelController.Models;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Services;

public class ConnectionService : IConnectionService {

    public ConnectionService(Profile profile) {
        Profile = profile;
        ConnectionInfo = profile.ActiveConnectionInfo;
    }

    private IDccClient? _dccClient;
    private Profile Profile { get; init; }
    public ConnectionInfo? ConnectionInfo { get; set; }

    public IDccClient? Connect() => Connect(Profile.ActiveConnectionInfo);
    public IDccClient? Connect(ConnectionInfo? connectionInfo) {
        if (connectionInfo is not null) ConnectionInfo = connectionInfo;
        ArgumentNullException.ThrowIfNull(ConnectionInfo);
        _dccClient = GetClient(ConnectionInfo);
        return _dccClient;
    }

    public IDccClient Connection => _dccClient ??= GetClient(ConnectionInfo);

    public void Disconnect() {
        if (_dccClient is not null) {
            _dccClient.Disconnect();
            _dccClient.TurnoutMsgReceived -= DccClientOnTurnoutMsgReceived;
            _dccClient.RouteMsgReceived -= DccClientOnRouteMsgReceived;
        }
        _dccClient = null;
    }
    
    private IDccClient GetClient(ConnectionInfo? connection = null) {
        
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (_dccClient != null) return _dccClient;
        
        // If we don't have a connection setup, and we did not provide settings, then we error
        // --------------------------------------------------------------------------
        if (connection?.Settings is null) throw new ArgumentNullException(nameof(connection),"No Connection Details provided.");
        
        // Attempt to connect to the Service provided and return the client
        // --------------------------------------------------------------------------
        _dccClient = DccClientFactory.Create(connection.Settings);
        var result = _dccClient.ConnectAsync().Result;
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
        route ??= Profile.Routes.FirstOrDefault(x => x.Id == e.RouteId) ?? null;
        route ??= Profile.Routes.FirstOrDefault(x => x.Id == e.DccAddress) ?? null;
        route ??= Profile.Routes.FirstOrDefault(x => x.Name == e.RouteId) ?? null;
        route ??= Profile.Routes.FirstOrDefault(x => x.Name == e.DccAddress) ?? null;
        if (route is not null) {
            Console.WriteLine($"Route {e.RouteId} updated");
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
        } else {
            Console.WriteLine($"Route {e.RouteId} added");
            Profile.Routes.Add(new Route() {
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
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.Id == e.TurnoutId) ?? null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.Id == e.DccAddress) ?? null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.Name == e.TurnoutId) ?? null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.Name == e.DccAddress) ?? null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.DccAddress == e.TurnoutId) ?? null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.DccAddress == e.DccAddress) ?? null;
        if (turnout is not null) {
            Console.WriteLine($"Turnout {e.TurnoutId} updated");
            turnout.State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown;
        } else if (Profile is not null && Profile.Turnouts is not null) {
            Console.WriteLine($"Turnout {e.TurnoutId} added");
            Profile.Turnouts.Add(new Turnout() {
                Name = e.TurnoutId,
                Id   = e.TurnoutId,
                DccAddress = e.DccAddress,
                State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown
            });
        }
    }

    public void CloseClient() {
        _dccClient?.Disconnect();
        _dccClient = null;
    }
}