using DccClients.Jmri;
using DccClients.Jmri.Client;
using DccClients.WiThrottle;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using Route = DCCPanelController.Models.DataModel.Route;

namespace DCCPanelController.Services;

public class ConnectionService {
    
    private bool _isInitialized = false;
    private readonly Profile _profile;
    public IDccClient? Client { get; private set; }

    public ConnectionService(Profile profile) {
        _profile = profile ?? throw new NullReferenceException("Profile cannot be null.");
        _ = Task.Run(async () => await InitializeConnectionAsync());
    }

    public event EventHandler<ConnectionMessageEvent>? ConnectionMessage;
    public event EventHandler<ConnectionChangedEvent>? ConnectionChanged;
    private bool IsConnected => Client is { IsConnected: true };

    public IDccClientSettings Settings {
        get {
            ArgumentNullException.ThrowIfNull(_profile,"Profile must be set. Fatal Error.");
            ArgumentNullException.ThrowIfNull(_profile.Settings,"Settings must exist. Fatal Error.");
            return _profile.Settings.ClientSettings ?? new JmriClientSettings();
        }
    }
    
    private async Task InitializeConnectionAsync() {
        if (_isInitialized) return;
        try {
            if (_profile.Settings.ConnectOnStartup) {
                var checkConnection = await ValidateConnectionAsync(Settings);
                if (checkConnection.IsFailure && Settings.SetAutomatically) {
                    var client = CreateClient(Settings);
                    if (client is not null) {
                        var findFirst = await client.GetAutomaticConnectionDetailsAsync();
                        if (findFirst is { IsSuccess: true, Value: not null }) {
                           _profile.Settings.ClientSettings = findFirst.Value;
                        }
                    }
                } 
                var result = await ConnectAsync(Settings);
                if (!result.IsSuccess) {
                    Console.WriteLine($"Background connection failed: {result.Message}");
                }
                _isInitialized = true;
            }
        } catch (Exception ex) {
            Console.WriteLine($"Background connection error: {ex.Message}");
        }
    }
    
    public async Task<IResult> ToggleConnectionAsync() {
        if (IsConnected) {
            await DisconnectAsync();
            return Result.Ok();
        }
        return await ConnectAsync(Settings);
    }

    public async Task DisconnectAsync() {
        if (Client is { }) {
            if (Client.IsConnected) await Client.DisconnectAsync();
            Client.ConnectionStateChanged -= OnConnectionChanged;
            Client.MessageReceived -= ClientOnMessageReceived;
            Client.OccupancyMsgReceived -= DccClientOnOccupancyMsgReceived;
            Client.TurnoutMsgReceived -= DccClientOnTurnoutMsgReceived;
            Client.RouteMsgReceived -= DccClientOnRouteMsgReceived;
            Client.SignalMsgReceived -= DccClientOnSignalMsgReceived;
            Client = null;
        }
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

    public async Task<IResult> ValidateConnectionAsync(IDccClientSettings clientSettings) {
        try {
            var client = CreateClient(clientSettings);
            if (client is null) return Result.Fail("Unable to create a Client instance.");
            return await client.ValidateConnectionAsync();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to connect to the server.").CausedBy(ex));
        }
    }

    private async Task<IResult> ConnectHelperAsync(IDccClientSettings clientSettings) {
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (Client is { IsConnected : true } && Client.Type == clientSettings.Type ) return Result.Ok();
        if (Client is { IsConnected : true }) await DisconnectAsync();
                
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

    private static IDccClient? CreateClient(IDccClientSettings clientSettings) {
        return clientSettings.Type switch {
            DccClientType.WiThrottle => new DccWiThrottleClient(clientSettings),
            DccClientType.Jmri       => new DccJmriClient(clientSettings),
            _                        => null
        };
    }
    
    private void OnConnectionChanged(object? sender = null, EventArgs? e = null) {
        var isConnected = IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
        ConnectionChanged?.Invoke(this, new ConnectionChangedEvent { Status = isConnected });
    }

    private void ClientOnMessageReceived(object? sender, DccMessageArgs e) {
        ConnectionMessage?.Invoke(this, new ConnectionMessageEvent { Message = e.Message });
    }

    public async Task<IResult> SendCmdAsync(string message) {
        return await Client?.SendCmdAsync(message)!;
    }

    public async Task<IResult> SendTurnoutCmdAsync(Turnout? turnout, bool thrown) {
        if (turnout is {Id: not null }) {
            return await Client?.SendTurnoutCmdAsync(turnout.Id, thrown)!;
        }
        return Result.Fail("No turnout provided.");
    }

    public async Task<IResult> SendRouteCmdAsync(Route? route, bool active) {
        if (route is { Id: not null }) {
            return await Client?.SendRouteCmdAsync(route.Id, active)!;
        }
        return Result.Fail("No route provided.");
    }

    public async Task<IResult> SendSignalCmdAsync(Signal? signal, SignalAspectEnum aspect) {
        if (signal is { Id: not null }) {
            return await Client?.SendSignalCmdAsync(signal.Id, aspect)!;
        }
        return Result.Fail("No signal provided.");
    }

    private void DccClientOnRouteMsgReceived(object? sender, DccRouteArgs e) {
        Route? route = null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Id == e.RouteId) ?? null;
        route ??= _profile.Routes.FirstOrDefault(x => x.Name == e.UserName) ?? null;
        if (route is not null) {
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
        } else {
            route = new Route {
                Id = e.RouteId,
                Name = e.UserName,
                State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive
            };
            _profile.Routes.Add(route);
        }
    }

    private void DccClientOnTurnoutMsgReceived(object? sender, DccTurnoutArgs e) {
        Turnout? turnout = null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.Id == e.TurnoutId) ?? null;
        turnout ??= _profile?.Turnouts?.FirstOrDefault(x => x.Name == e.Username) ?? null;
        if (turnout is not null) {
            turnout.State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown;
        } else if (_profile is not null && _profile.Turnouts is not null) {
            turnout = new Turnout {
                Id = e.TurnoutId,
                Name = e.Username,
                DccAddress = e.TurnoutId.FromDccAddressString(),
                State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown
            };
            _profile.Turnouts.Add(turnout);
        }
    }

    private void DccClientOnOccupancyMsgReceived(object? sender, DccOccupancyArgs e) {
        Block? block = null;
        block ??= _profile?.Blocks?.FirstOrDefault(x => x.Id == e.BlockId) ?? null;
        block ??= _profile?.Blocks?.FirstOrDefault(x => x.Name == e.UserName) ?? null;
        if (block is not null) {
            block.IsOccupied = e.IsOccupied;
        } else if (_profile is not null && _profile.Blocks is not null) {
            block = new Block {
                Id = e.BlockId,
                Name = e.UserName,
                IsOccupied = e.IsOccupied
            };
            _profile.Blocks.Add(block);
        }
    }

    private void DccClientOnSignalMsgReceived(object? sender, DccSignalArgs e) {
        Signal? signal = null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Id == e.SignalId) ?? null;
        signal ??= _profile?.Signals?.FirstOrDefault(x => x.Name == e.SignalId) ?? null;
        if (signal is not null) {
            signal.Aspect = e.Aspect;
        } else if (_profile is not null && _profile.Signals is not null) {
            signal = new Signal {
                Id = e.SignalId,
                Name = e.SignalId,
                DccAddress = e.SignalId.FromDccAddressString(),
                Aspect = e.Aspect
            };
            _profile.Signals.Add(signal);
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