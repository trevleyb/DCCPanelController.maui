using DCCClients;
using DCCClients.Jmri;
using DCCClients.WiThrottle;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Events;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using ConnectionInfo = DCCPanelController.Models.DataModel.ConnectionInfo;
using Route = DCCPanelController.Models.DataModel.Route;

namespace DCCPanelController.Services;

public class ConnectionService : IDccClientCommands {
    
    private readonly Profile _profile;
    private readonly ConnectionInfo? _connectionInfo;
    private IDccClient? _client;

    public ConnectionService(Profile profile) {
        _profile = profile;
        _connectionInfo = profile.ActiveConnectionInfo;
    }

    public event EventHandler<ConnectionMessageEvent>? ConnectionMessage;
    public event EventHandler<ConnectionChangedEvent>? ConnectionChanged;

    public bool IsConnected => (_client is { IsConnected: true }); 
    public string ConnectionIcon => IsConnected ? "wifi.png" : "wifi_off.png";
    
    public async Task<IResult> ToggleConnectionAsync() {
        if (IsConnected) {
            await DisconnectAsync();
            return Result.Ok();
        }
        return await ConnectAsync(_connectionInfo);
    }
    
    public async Task DisconnectAsync() {
        Console.WriteLine("Disconnecting");
        if (_client is not null) {
            _client.TurnoutMsgReceived -= DccClientOnTurnoutMsgReceived;
            _client.RouteMsgReceived -= DccClientOnRouteMsgReceived;
            _client.SignalMsgReceived -= DccClientOnSignalMsgReceived;
            _client.OccupancyMsgReceived -= DccClientOnOccupancyMsgReceived;
            await _client.DisconnectAsync();
        }
        _client = null;
        OnConnectionChanged();
    }

    public async Task<IResult> ConnectAsync(ConnectionInfo? connection = null) {
        try {
            var result = await ConnectHelper(connection);
            Console.WriteLine($"Connection Result: {result.IsSuccess} {result.Message}");
            OnConnectionChanged();
            return result;
        } catch (Exception ex) {
            Console.WriteLine($"Connection Failed: {ex.Message}");
            OnConnectionChanged();
            return Result.Fail("Unable to connect to the server.", ex);
        }     
    }

    private async Task<IResult> ConnectHelper(ConnectionInfo? connection = null) {
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (_client is { IsConnected: true }) return Result.Ok();
        if (connection is {Settings : null }) return Result.Fail("No Connection Details provided.");
        ArgumentNullException.ThrowIfNull(connection);
        
        // Attempt to connect to the Service provided and return the client
        // --------------------------------------------------------------------------
        _client = CreateClient(connection.Settings);
        if (_client is null) return Result.Fail("Unable to create a Client instance.");
        
        var result = await _client.ConnectAsync();
        if (result.IsFailure) return Result.Fail("Unable to connect to the specified server.");
        
        _client.ConnectionStateChanged  += OnConnectionChanged;
        _client.MessageReceived         += ClientOnMessageReceived;
        _client.OccupancyMsgReceived    += DccClientOnOccupancyMsgReceived;
        _client.TurnoutMsgReceived      += DccClientOnTurnoutMsgReceived;
        _client.RouteMsgReceived        += DccClientOnRouteMsgReceived;
        _client.SignalMsgReceived       += DccClientOnSignalMsgReceived;
        
        return Result.Ok("Connected to the Server.");
    }

    private void OnConnectionChanged(object? sender = null, EventArgs? e = null) {
        var isConnected = IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
        ConnectionChanged?.Invoke(this, new ConnectionChangedEvent { Status = isConnected });
    }
    
    private void ClientOnMessageReceived(object? sender, DccMessageArgs e) {
        ConnectionMessage?.Invoke(this, new ConnectionMessageEvent { Message = e.Message});
    }

    private static IDccClient?CreateClient(IDccSettings settings) {
        return settings.Type.ToLowerInvariant() switch {
            "withrottle" => new DccWiThrottleClient(settings),
            "jmri"       => new DccJmriClient(settings),
            _            => null
        };
    }
    
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
                Name = string.IsNullOrEmpty(e.TurnoutId) ? e.DccAddress : e.TurnoutId,
                Id = string.IsNullOrEmpty(e.TurnoutId) ? e.DccAddress : e.TurnoutId,
                DccAddress = e.DccAddress,
                State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown
            };
            _profile.Turnouts.Add(turnout);
            Console.WriteLine($"Turnout Added {turnout.Name} is now {turnout.State}");
        }
    }
    
    private void DccClientOnOccupancyMsgReceived(object? sender, DccOccupancyArgs e) {
        Block? block = null;
        block ??= _profile?.Blocks?.FirstOrDefault(x => x.Id == e.BlockId) ?? null;
        block ??= _profile?.Blocks?.FirstOrDefault(x => x.Name == e.BlockId) ?? null;
        if (block is not null) {
            block.IsOccupied = e.IsOccupied;
            Console.WriteLine($"Block Updated {block.Name} is now {(block.IsOccupied ? "OCCUPIED" : "FREE")}");
        } else if (_profile is not null && _profile.Blocks is not null) {
            block = new Block {
                Name = e.BlockId,
                Id = e.BlockId,
                IsOccupied = e.IsOccupied
            };
            _profile.Blocks.Add(block);
            Console.WriteLine($"Block Added {block.Name} is now {(block.IsOccupied ? "OCCUPIED" : "FREE")}");
        }
    }
    
    private void DccClientOnSignalMsgReceived(object? sender, DccSignalArgs e) {
        Signal? signal = null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Id == e.SignalId) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Name == e.SignalId) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.DccAddress == e.SignalId) ?? null;
        if (signal is not null) {
            signal.Aspect = e.Aspect;
            Console.WriteLine($"Signal Updated {signal.Name} is now {signal.Aspect}");
        } else if (_profile is not null && _profile.Signals is not null) {
            signal = new Signal {
                Name = e.SignalId,
                Id = e.SignalId,
                DccAddress = e.SignalId,
                Aspect = e.Aspect
            };
            _profile.Signals.Add(signal);
            Console.WriteLine($"Signal Added {signal.Name} with aspect {signal.Aspect}");
        }
    }

    public async Task<IResult> SendCmdAsync(string message) => await _client?.SendCmdAsync(message)!;
    public async Task<IResult> SendTurnoutCmdAsync(string dccAddress, bool thrown) => await _client?.SendTurnoutCmdAsync(dccAddress, thrown)!;
    public async Task<IResult> SendRouteCmdAsync(string dccAddress, bool active) => await _client?.SendRouteCmdAsync(dccAddress, active)!;
    public async Task<IResult> SendSignalCmdAsync(string dccAddress, SignalAspectEnum aspect) => await _client?.SendSignalCmdAsync(dccAddress, aspect)!;
    public async Task<IResult> ForceRefresh(string? type = "all") => await _client?.ForceRefreshAsync(type)!;
}

public class ConnectionChangedEvent : EventArgs {
    public ConnectionStatus Status { get; init; }
    public bool IsConnected => Status == ConnectionStatus.Connected;
    public string ConnectionIcon => IsConnected ? "wifi.png" : "wifi_off.png";
}

public class ConnectionMessageEvent : EventArgs {
    public string Message { get; init; } = string.Empty;
}