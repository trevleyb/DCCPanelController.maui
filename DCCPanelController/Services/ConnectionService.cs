using DccClients.Jmri;
using DccClients.Jmri.Client;
using DccClients.WiThrottle;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Events;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using Route = DCCPanelController.Models.DataModel.Route;

namespace DCCPanelController.Services;

public class ConnectionService {
    
    private readonly Profile _profile;
    private readonly IDccClientSettings _clientSettings;
    public IDccClient? Client { get; private set; }

    public ConnectionService(Profile profile) {
        _profile = profile ?? throw new NullReferenceException("Profile cannot be null.");
        _clientSettings = _profile.Settings?.ClientSettings ?? new JmriClientSettings();
    }

    public event EventHandler<ConnectionMessageEvent>? ConnectionMessage;
    public event EventHandler<ConnectionChangedEvent>? ConnectionChanged;

    public string ConnectionIcon => IsConnected ? "wifi.png" : "wifi_off.png";
    public bool IsConnected => Client is { IsConnected: true };

    public async Task<IResult> ToggleConnectionAsync() {
        if (IsConnected) {
            await DisconnectAsync();
            return Result.Ok();
        }
        return await ConnectAsync(_clientSettings);
    }

    public async Task DisconnectAsync() {
        Console.WriteLine("Disconnecting");
        if (Client is not null) {
            Client.TurnoutMsgReceived -= DccClientOnTurnoutMsgReceived;
            Client.RouteMsgReceived -= DccClientOnRouteMsgReceived;
            Client.SignalMsgReceived -= DccClientOnSignalMsgReceived;
            Client.OccupancyMsgReceived -= DccClientOnOccupancyMsgReceived;
            await Client.DisconnectAsync();
        }
        Client = null;
        OnConnectionChanged();
    }

    public async Task<IResult> ConnectAsync(IDccClientSettings clientSettings) {
        try {
            var result = await ConnectHelperAsync(clientSettings);
            Console.WriteLine($"Connection Result: {result.IsSuccess} {result.Message}");
            OnConnectionChanged();
            return result;
        } catch (Exception ex) {
            Console.WriteLine($"Connection Failed: {ex.Message}");
            OnConnectionChanged();
            return Result.Fail("Unable to connect to the server.", ex);
        }
    }

    private async Task<IResult> ConnectHelperAsync(IDccClientSettings clientSettings) {
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (Client is { IsConnected              : true }) return Result.Ok();

        // Attempt to connect to the Service provided and return the client
        // --------------------------------------------------------------------------
        Client = CreateClient(clientSettings);
        if (Client is null) return Result.Fail("Unable to create a Client instance.");

        var result = await Client.ConnectAsync();
        if (result.IsFailure) return Result.Fail("Unable to connect to the specified server.");

        Client.ConnectionStateChanged += OnConnectionChanged;
        Client.MessageReceived += ClientOnMessageReceived;
        Client.OccupancyMsgReceived += DccClientOnOccupancyMsgReceived;
        Client.TurnoutMsgReceived += DccClientOnTurnoutMsgReceived;
        Client.RouteMsgReceived += DccClientOnRouteMsgReceived;
        Client.SignalMsgReceived += DccClientOnSignalMsgReceived;

        return Result.Ok("Connected to the Server.");
    }

    private void OnConnectionChanged(object? sender = null, EventArgs? e = null) {
        var isConnected = IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
        ConnectionChanged?.Invoke(this, new ConnectionChangedEvent { Status = isConnected });
    }

    private void ClientOnMessageReceived(object? sender, DccMessageArgs e) {
        Console.WriteLine($"ClientOnMessageReceived called");
        ConnectionMessage?.Invoke(this, new ConnectionMessageEvent { Message = e.Message });
    }

    private static IDccClient? CreateClient(IDccClientSettings clientSettings) {
        return clientSettings.Type switch {
            DccClientType.WiThrottle => new DccWiThrottleClient(clientSettings),
            DccClientType.Jmri       => new DccJmriClient(clientSettings),
            _                        => null
        };
    }

    public async Task<IResult> SendCmdAsync(string message) {
        return await Client?.SendCmdAsync(message)!;
    }

    public async Task<IResult> SendTurnoutCmdAsync(string dccAddress, bool thrown) {
        return await Client?.SendTurnoutCmdAsync(dccAddress, thrown)!;
    }

    public async Task<IResult> SendRouteCmdAsync(string dccAddress, bool active) {
        return await Client?.SendRouteCmdAsync(dccAddress, active)!;
    }

    public async Task<IResult> SendSignalCmdAsync(string dccAddress, SignalAspectEnum aspect) {
        return await Client?.SendSignalCmdAsync(dccAddress, aspect)!;
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

    public async Task<IResult> ForceRefresh(string? type = "all") {
        return await Client?.ForceRefreshAsync(type)!;
    }
}

public class ConnectionChangedEvent : EventArgs {
    public ConnectionStatus Status { get; init; }
    public bool IsConnected => Status == ConnectionStatus.Connected;
    public string ConnectionIcon => IsConnected ? "wifi.png" : "wifi_off.png";
}

public class ConnectionMessageEvent : EventArgs {
    public string Message { get; init; } = string.Empty;
}