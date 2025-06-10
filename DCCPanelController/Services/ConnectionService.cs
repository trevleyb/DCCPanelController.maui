using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DccClients.Jmri;
using DccClients.Jmri.Client;
using DccClients.WiThrottle;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using Route = DCCPanelController.Models.DataModel.Route;

namespace DCCPanelController.Services;

public partial class ConnectionService : ObservableObject {
    private const int MaxServerMessages = 500;

    [ObservableProperty] private Profile _profile;
    private IDccClient? Client { get; set; }
    private IDccClientSettings Settings => Profile.Settings.ClientSettings ?? new UnknownSettings();
    public event EventHandler<ConnectionChangedEvent>? ConnectionChanged;
    public bool IsConnected => Client is { IsConnected: true };

    [ObservableProperty] private ObservableCollection<ServerMessage> _serverMessages = [];

    public ConnectionService(Profile profile) {
        Profile = profile ?? throw new NullReferenceException("Profile cannot be null.");
        _ = Task.Run(async () => await InitializeConnectionAsync());
    }

    private async Task InitializeConnectionAsync() {
        try {
            if (Profile.Settings.ConnectOnStartup) await ConnectAsync(Settings);
        } catch (Exception ex) {
            Console.WriteLine($"Initialisation connection error: {ex.Message}");
        }
    }

    public async Task<IResult> ToggleConnectionAsync() {
        if (IsConnected) {
            await DisconnectAsync();
            return Result.Ok();
        }
        return await ConnectAsync(Settings);
    }

    public async Task<IResult> ConnectAsync(IDccClientSettings clientSettings) {
        try {
            // Allow null as a parameter if we know we have already setup the connection
            // This will then return the active connection.
            // --------------------------------------------------------------------------
            if (Client is { IsConnected : true }) await DisconnectAsync();

            // Attempt to connect to the Service provided and return the client
            // --------------------------------------------------------------------------
            Client = CreateClient(clientSettings);
            if (Client is null) {
                OnConnectionChanged();
                return Result.Fail("Unable to create a Client instance.");
            }

            if (Settings.SetAutomatically) {
                var findFirst = await Client.GetAutomaticConnectionDetailsAsync();
                if (findFirst is { IsSuccess: true, Value: not null }) {
                    Profile.Settings.ClientSettings = findFirst.Value;
                }
            }

            var connectResult = await Client.ConnectAsync();
            if (connectResult.IsFailure) return Result.Fail("Unable to connect to the specified server.");

            Client.ConnectionStateChanged += OnConnectionChanged;
            Client.MessageReceived += ClientOnMessageReceived;
            Client.OccupancyMsgReceived += DccClientOnOccupancyMsgReceived;
            Client.TurnoutMsgReceived += DccClientOnTurnoutMsgReceived;
            Client.RouteMsgReceived += DccClientOnRouteMsgReceived;
            Client.SignalMsgReceived += DccClientOnSignalMsgReceived;

            OnConnectionChanged();
            await SetTurnoutsToDefaultState();
            return connectResult;
        } catch (Exception ex) {
            Console.WriteLine($"Connection Failed: {ex.Message}");
            OnConnectionChanged();
            return Result.Fail("Unable to connect to the server.", ex);
        }
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

    public async Task SetTurnoutsToDefaultState() {
        if (Profile.Settings.SetTurnoutStatesOnStartup) {
            if (Client is { IsConnected: true } ) {
                foreach (var turnout in Profile.Turnouts) {
                    if (turnout.Default != TurnoutStateEnum.Unknown && !string.IsNullOrEmpty(turnout?.Id)) {
                        await Client.SendTurnoutCmdAsync(turnout.Id, turnout.State == TurnoutStateEnum.Thrown)!;;
                    }
                }
            }
        }
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
        AddMessage($"Server Connection Changed to {(IsConnected ? "Connected" : "Disconnected")}","Connection");
        ConnectionChanged?.Invoke(this, new ConnectionChangedEvent { Status = isConnected });
    }

    private void ClientOnMessageReceived(object? sender, DccMessageArgs e) {
        AddMessage($"{e.Message}","Message");
    }

    public async Task<IResult> SendCmdAsync(string message) {
        AddMessage($"{message}","Command");
        return await Client?.SendCmdAsync(message)!;
    }

    public async Task<IResult> SendTurnoutCmdAsync(Turnout? turnout, bool thrown) {
        AddMessage($"Set {turnout?.Id} to '{turnout?.State}'","Turnout");
        if (turnout is { Id: not null }) {
            return await Client?.SendTurnoutCmdAsync(turnout.Id, thrown)!;
        }
        return Result.Fail("No turnout provided.");
    }

    public async Task<IResult> SendRouteCmdAsync(Route? route, bool active) {
        AddMessage($"Set {route?.Id} to '{route?.State}'","Route");
        if (route is { Id: not null }) {
            return await Client?.SendRouteCmdAsync(route.Id, active)!;
        }
        return Result.Fail("No route provided.");
    }

    public async Task<IResult> SendBlockCmdAsync(Block? block, bool isOccupied) {
        AddMessage($"Set {block?.Id} to '{(isOccupied ? "Occupied" : "Free")}'","Block");
        if (block is { Sensor: not null }) {
            return await Client?.SendBlockCmdAsync(block.Sensor, isOccupied)!;
        }
        return Result.Fail("No block(sensor) provided.");
    }
    
    public async Task<IResult> SendSignalCmdAsync(Signal? signal, SignalAspectEnum aspect) {
        AddMessage($"Set {signal?.Id} to {signal?.Aspect}","Signal");
        if (signal is { Id: not null }) {
            return await Client?.SendSignalCmdAsync(signal.Id, aspect)!;
        }
        return Result.Fail("No signal provided.");
    }

    private void DccClientOnRouteMsgReceived(object? sender, DccRouteArgs e) {
        AddMessage($"Received Route {e.RouteId} with state {(e.IsActive ? "'Active'" : "'Inactive'")}","Route");

        Route? route = null;
        route ??= Profile.Routes.FirstOrDefault(x => x.Id == e.RouteId) ?? null;
        route ??= Profile.Routes.FirstOrDefault(x => x.Name == e.UserName) ?? null;
        if (route is not null) {
            route.State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive;
        } else {
            route = new Route {
                Id = e.RouteId,
                Name = e.UserName,
                State = e.IsActive ? RouteStateEnum.Active : RouteStateEnum.Inactive
            };
            Profile.Routes.Add(route);
        }
    }

    private void DccClientOnTurnoutMsgReceived(object? sender, DccTurnoutArgs e) {
        AddMessage($"Received Turnout {e.TurnoutId} with state {(e.IsClosed ? "'Closed'" : "'Thrown'")}","Turnout");
        
        Turnout? turnout = null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.Id == e.TurnoutId) ?? null;
        turnout ??= Profile?.Turnouts?.FirstOrDefault(x => x.Name == e.Username) ?? null;
        if (turnout is not null) {
            turnout.State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown;
        } else if (Profile is not null && Profile.Turnouts is not null) {
            turnout = new Turnout {
                Id = e.TurnoutId,
                Name = e.Username,
                DccAddress = e.TurnoutId.FromDccAddressString(),
                State = e.IsClosed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown
            };
            Profile.Turnouts.Add(turnout);
        }
    }

    private void DccClientOnOccupancyMsgReceived(object? sender, DccOccupancyArgs e) {
        AddMessage($"Received Occupancy {e.BlockId} with state {(e.IsOccupied ? "'Occupied'" : "'Free'")}","Occupancy");

        Block? block = null;
        block ??= Profile?.Blocks?.FirstOrDefault(x => x.Id == e.BlockId) ?? null;
        block ??= Profile?.Blocks?.FirstOrDefault(x => x.Name == e.UserName) ?? null;
        if (block is not null) {
            block.IsOccupied = e.IsOccupied;
        } else if (Profile is not null && Profile.Blocks is not null) {
            block = new Block {
                Id = e.BlockId,
                Name = e.UserName,
                Sensor = e.Sensor,
                IsOccupied = e.IsOccupied
            };
            Profile.Blocks.Add(block);
        }
    }

    private void DccClientOnSignalMsgReceived(object? sender, DccSignalArgs e) {
        AddMessage($"Received Signal {e.SignalId} with aspect '{e.Aspect}'","Signal");

        Signal? signal = null;
        signal ??= Profile?.Signals?.FirstOrDefault(x => x.Id == e.SignalId) ?? null;
        signal ??= Profile?.Signals?.FirstOrDefault(x => x.Name == e.SignalId) ?? null;
        if (signal is not null) {
            signal.Aspect = e.Aspect;
        } else if (Profile is not null && Profile.Signals is not null) {
            signal = new Signal {
                Id = e.SignalId,
                Name = e.SignalId,
                DccAddress = e.SignalId.FromDccAddressString(),
                Aspect = e.Aspect
            };
            Profile.Signals.Add(signal);
        }
    }

    public async Task<IResult> ForceRefresh(string? type = "all") {
        return await Client?.ForceRefreshAsync(type)!;
    }

    [RelayCommand]
    public void ClearMessages() {
        ServerMessages.Clear();
        OnPropertyChanged(nameof(ServerMessages));
    }

    public void AddMessage(string message, string operation = "") {
        var msg = new ServerMessage(message, operation);
        ServerMessages.Insert(0, msg);
        while (ServerMessages.Count > MaxServerMessages) {
            ServerMessages.RemoveAt(ServerMessages.Count - 1);
        }
        OnPropertyChanged(nameof(ServerMessages));
    }
}

public class ConnectionChangedEvent : EventArgs {
    public ConnectionStatus Status { get; init; }
    public bool IsConnected => Status == ConnectionStatus.Connected;
}

public partial class ServerMessage(string message, string operation = "") : ObservableObject {
    [ObservableProperty] private string _message = message;
    [ObservableProperty] private string _operation = operation;
    [ObservableProperty] private DateTime _timeStamp = DateTime.Now;
}