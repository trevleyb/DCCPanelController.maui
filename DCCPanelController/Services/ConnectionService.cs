using DCCClients;
using DCCClients.Events;
using DCCClients.Interfaces;
using DCCClients.Jmri;
using DCCClients.WiThrottle;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using ConnectionInfo = DCCPanelController.Models.DataModel.ConnectionInfo;

namespace DCCPanelController.Services;

public sealed class ConnectionService {
    private readonly Profile _profile;
    private ConnectionInfo? _connectionInfo;

    private IDccClient? _dccClient;

    public ConnectionService(Profile profile) {
        _profile = profile;
        _connectionInfo = profile.ActiveConnectionInfo;
    }

    public bool IsConnected => _dccClient is not null && _dccClient.IsConnected;

    public event EventHandler<ConnectionChangedEvent>? ConnectionChanged;

    public async Task<IDccClient?> Connect() {
        return await Connect(_profile.ActiveConnectionInfo);
    }

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
            _dccClient.SignalMsgReceived -= DccClientOnSignalMsgReceived;
        }
        _dccClient = null;
        OnConnectionChanged();
    }

    private async Task<IDccClient> GetClient(ConnectionInfo? connection = null) {
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (_dccClient is { IsConnected: true }) return _dccClient;
        if (_dccClient is { IsConnected: false }) Disconnect();

        // If we don't have a connection setup, and we did not provide settings, then we error
        // --------------------------------------------------------------------------
        if (connection?.Settings is null) throw new ArgumentNullException(nameof(connection), "No Connection Details provided.");

        // Attempt to connect to the Service provided and return the client
        // --------------------------------------------------------------------------
        _dccClient = CreateClient(connection.Settings);
        var result = await _dccClient.ConnectAsync();
        if (result.IsFailure) _dccClient = new DccInvalidClient(connection.Settings);

        _dccClient.TurnoutMsgReceived += DccClientOnTurnoutMsgReceived;
        _dccClient.RouteMsgReceived += DccClientOnRouteMsgReceived;
        _dccClient.SignalMsgReceived += DccClientOnSignalMsgReceived;
        return _dccClient;
    }

    /// <summary>
    ///     Global Update of any routes that get sent by the server
    /// </summary>
    private void DccClientOnRouteMsgReceived(object? sender, DccRouteArgs e) {
        Route? route = null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Id == e.RouteId) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Id == e.DccAddress) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Name == e.RouteId) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Name == e.DccAddress) ?? null;
        if (route is not null) {
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
            Console.WriteLine($"Route Updated {route.Name} is now {route.State}");
        } else {
            route = new Route {
                Id = e.RouteId,
                Name = e.DccAddress,
                State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive
            };
            _profile.Routes.Add(route);
            Console.WriteLine($"Route Added {route.Name} is now {route.State}");
        }
    }

    /// <summary>
    ///     Global update of any Turnouts that appear once we do a connection. WiThrottle in particular only reports
    ///     any defined turnouts on connection.
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
            turnout.State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown;
            Console.WriteLine($"Turnout Updated {turnout.Name} is now {turnout.State}");
        } else if (_profile is not null && _profile.Turnouts is not null) {
            turnout = new Turnout {
                Name = e.TurnoutId,
                Id = e.TurnoutId,
                DccAddress = e.DccAddress,
                State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown
            };
            _profile.Turnouts.Add(turnout);
            Console.WriteLine($"Turnout Added {turnout.Name} is now {turnout.State}");
        }
    }
    
    /// <summary>
    ///     Global update of any Signals that appear once we do a connection.
    /// </summary>
    private void DccClientOnSignalMsgReceived(object? sender, DccSignalArgs e) {
        Signal? signal = null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Id == e.SignalId) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Id == e.DccAddress) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Name == e.SignalId) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Name == e.DccAddress) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.DccAddress == e.SignalId) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.DccAddress == e.DccAddress) ?? null;
        if (signal is not null) {
            signal.Aspect = e.Aspect;
            Console.WriteLine($"Signal Updated {signal.Name} is now {signal.Aspect}");
        } else if (_profile is not null && _profile.Signals is not null) {
            signal = new Signal {
                Name = e.SignalId,
                Id = e.SignalId,
                DccAddress = e.DccAddress,
                Aspect = e.Aspect
            };
            _profile.Signals.Add(signal);
            Console.WriteLine($"Signal Added {signal.Name} with aspect {signal.Aspect}");
        }
    }

    private void OnConnectionChanged() {
        ConnectionChanged?.Invoke(this, new ConnectionChangedEvent { IsConnected = IsConnected });
    }
    
    public static IDccClient CreateClient(IDccSettings settings) {
        return settings.Type.ToLowerInvariant() switch {
            "withrottle" => new DccWiThrottleClient(settings),
            "jmri"       => new DccJmriClient(settings),
            _            => throw new NotImplementedException("Unknown client requested: {name}")
        };
    }
}
