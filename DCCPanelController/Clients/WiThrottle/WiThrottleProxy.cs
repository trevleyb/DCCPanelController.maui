using System.Data;
using DccClients.WiThrottle;
using DccClients.WiThrottle.Client;
using DccClients.WiThrottle.Client.Commands;
using DccClients.WiThrottle.Client.Events;
using DCCCommon;
using DCCCommon.Discovery;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Clients.WiThrottle;

public class WiThrottleProxy : DccClientBase, IDccClient {
    private readonly Profile            _profile;
    private readonly WiThrottleSettings _settings;
    private          WiThrottleClient?  _client;

    public WiThrottleProxy(Profile profile, IDccClientSettings clientSettings) : base(profile) {
        _profile = profile;
        _settings = clientSettings as WiThrottleSettings ?? throw new InvalidCastException("Incorrect Settings Type provided for WiThrottle");
    }

    public static List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes];
    public DccClientType Type => DccClientType.Simulator;

    #region Connect and Disconnect Methods
    public async Task<IResult> ConnectAsync() {
        if (_client is { }) await DisconnectAsync();
        Status = DccClientStatus.Initialising;

        if (_settings.SetAutomatically) {
            var result = await GetAutomaticConnectionDetailsAsync();
            if (result.IsFailure) {
                Status = DccClientStatus.Disconnected;
                OnClientMessage("Could not automatically set connection details.");
                return result;
            }
            if (result.Value is WiThrottleSettings jmriSettings) {
                _settings.Address = jmriSettings.Address;
                _settings.Port = jmriSettings.Port;
            }
        }
        _profile?.RefreshAll();
        try {
            _client = new WiThrottleClient(_settings.Name, _settings.Address, _settings.Port);
            await _client.ConnectAsync();
            if (_client is null || !_client.IsRunning) throw new ApplicationException("Unable to connect to WiThrottle server.");

            _client.ConnectionEvent += ClientOnConnectionEvent;
            _client.DataEvent += ClientOnDataEvent;
            Status = DccClientStatus.Connected;
            OnClientMessage("Connected to Jmri");
            return Result.Ok();
        } catch (Exception ex) {
            OnClientMessage(ex.Message);
            return Result.Fail(ex.Message);
        }
    }

    public async Task<IResult> DisconnectAsync() {
        Status = DccClientStatus.Disconnected;
        if (_client is { }) {
            _client.ConnectionEvent -= ClientOnConnectionEvent;
            _client.DataEvent -= ClientOnDataEvent;
        }
        OnClientMessage("Disconnected from WiThrottle");
        return Result.Ok();
    }

    /// <summary>
    ///     Find any servers on the network and attaches to the first one it finds
    ///     or returns a Result.Failed if it could not find any server.
    /// </summary>
    public async Task<IResult> SetAutomaticSettingsAsync() {
        var settings = await GetAutomaticConnectionDetailsAsync();
        if (settings.IsSuccess) {
            if (settings.Value is WiThrottleSettings withrottleSettings) {
                _settings.Address = withrottleSettings.Address;
                _settings.Port = withrottleSettings.Port;
            } else {
                return Result.Fail("Unable to find any WiThrottle servers. ");
            }
        }
        return Result.Ok();
    }

    /// <summary>
    ///     Forces a refresh of the data from the WiThrottleServer. The server only sends
    ///     throttle and routes on initial start-up. So to refresh, we need to disconnect,
    ///     wait 1/2 seconds and reconnect.
    /// </summary>
    public async Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null) {
        if (_client is { }) _client.Disconnect();
        var connected = await ConnectAsync();
        await Task.Delay(500);
        if (connected.IsSuccess && Status == DccClientStatus.Connected) return Result.Ok();
        return Result.Fail("Unable to force refresh data from WiThrottle server.");
    }

    /// <summary>
    ///     This method attempts to connect to the server and then disconnects.
    ///     It returns either OK or FAILED based on attempting to connect.
    /// </summary>
    public async Task<IResult> ValidateConnectionAsync() {
        if (_client is { }) await DisconnectAsync();
        var connected = await ConnectAsync();
        await Task.Delay(500);
        if (connected.IsSuccess && Status == DccClientStatus.Connected) await DisconnectAsync();
        return connected;
    }
    #endregion

    #region Send Commands
    public async Task<IResult> SendTurnoutCmdAsync(Turnout turnout, bool thrown) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to WiThrottle server");
        if (string.IsNullOrEmpty(turnout.Id)) return Result.Fail("Invalid Turnout Id provided.");
        try {
            _client.SendMessage(new TurnoutCommand(turnout.Id, thrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed));
            OnClientMessage($"Setting turnout {turnout.Name}({turnout.Id}) to {(thrown ? "THROWN" : "CLOSED")}", DccClientOperation.Turnout, DccClientMessageType.Outbound);
            await Task.CompletedTask;
            return Result.Ok();
        } catch (Exception ex) {
            await Task.CompletedTask;
            return Result.Fail(ex, "Failed to send turnout command to WiThrottle server");
        }
    }

    public async Task<IResult> SendRouteCmdAsync(Route route, bool active) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to WiThrottle server");
        if (string.IsNullOrEmpty(route.Id)) return Result.Fail("Invalid Route Id provided.");
        try {
            _client.SendMessage(new RouteCommand(route.Id));
            OnClientMessage($"Setting route {route.Name}({route.Id}) to {(active ? "ACTIVE" : "INACTIVE")}", DccClientOperation.Route, DccClientMessageType.Outbound);
            await Task.CompletedTask;
            return Result.Ok();
        } catch (Exception ex) {
            await Task.CompletedTask;
            return Result.Fail(ex, "Failed to send route command to WiThrottle server");
        }
    }

    public async Task<IResult> SendSignalCmdAsync(Signal signal, SignalAspectEnum aspect) {
        await Task.CompletedTask;
        return Result.Fail("Signal commands are not supported by WiThrottle.");
    }

    public async Task<IResult> SendLightCmdAsync(Light light, bool isActive) {
        await Task.CompletedTask;
        return Result.Fail("Light commands are not supported by WiThrottle.");
    }

    public async Task<IResult> SendBlockCmdAsync(Block block, bool isOccupied) {
        await Task.CompletedTask;
        return Result.Fail("Block commands are not supported by WiThrottle.");
    }

    public async Task<IResult> SendSensorCmdAsync(Sensor sensor, bool isOccupied) {
        await Task.CompletedTask;
        return Result.Fail("Sensor commands are not supported by WiThrottle.");
    }
    #endregion

    #region Manage Events from the Client
    private void ClientOnDataEvent(IClientEvent clientEvent) {
        switch (clientEvent) {
            case MessageEvent message:
                OnClientMessage(message.Value, DccClientOperation.System, DccClientMessageType.Inbound);
            break;

            case TurnoutEvent turnout:
                OnClientMessage($"Turnout Change Event: {turnout.SystemName}=>{turnout.State}", DccClientOperation.Turnout, DccClientMessageType.Inbound);
                UpdateTurnout(turnout.SystemName, turnout.UserName, turnout.StateEnum == TurnoutStateEnum.Thrown ? Models.DataModel.Entities.TurnoutStateEnum.Thrown : Models.DataModel.Entities.TurnoutStateEnum.Closed);
            break;

            case RouteEvent route:
                OnClientMessage($"Route Change Event: {route.SystemName}=>{route.State}", DccClientOperation.Route, DccClientMessageType.Inbound);
                UpdateRoute(route.SystemName, route.UserName, route.StateEnum == RouteStateEnum.Active ? Models.DataModel.Entities.RouteStateEnum.Active : Models.DataModel.Entities.RouteStateEnum.Inactive);
            break;

            default:
                OnClientMessage($"Received an invalid Message from WiThrottle '{clientEvent.GetType().Name}' {clientEvent.ToString()}");
            break;
        }
    }

    private void ClientOnConnectionEvent(IClientEvent clientEvent) {
        if (clientEvent.GetType() == typeof(ConnectionEvent)) {
            Status = ((ConnectionEvent)clientEvent).State switch {
                ConnectionState.Open       => DccClientStatus.Connected,
                ConnectionState.Closed     => DccClientStatus.Disconnected,
                ConnectionState.Connecting => DccClientStatus.Initialising,
                _                          => DccClientStatus.Disconnected,
            };
            OnClientMessage("Connection to WiThrottle server");
        }
    }
    #endregion

    #region Discover Methods
    public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
        WiThrottleSettings settings = new();

        var findServers = await FindAvailableServicesAsync();
        if (findServers.IsFailure) return Result<IDccClientSettings?>.Fail("No available servers could be found.");

        var firstServer = findServers.Value?.FirstOrDefault();
        if (firstServer is null) return Result<IDccClientSettings?>.Fail("Unable to set connection automatically. No WiThrottle servers found.");

        settings.Address = firstServer.Address.ToString();
        settings.Port = firstServer.Port;
        return Result<IDccClientSettings?>.Ok(settings);
    }

    public async Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() {
        try {
            var result = await DiscoverServices.SearchForJmriServicesAsync();
            if (result is { IsSuccess: true, Value.Count: > 0 }) return result;
        } catch (Exception ex) {
            return Result<List<DiscoveredService>>.Fail(ex, "Unable to find a WiThrottle server.");
        }
        return Result<List<DiscoveredService>>.Fail("Unable to find a WiThrottle server.");
    }
    #endregion
}