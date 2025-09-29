using DccClients.Jmri;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;
using DCCCommon;
using DCCCommon.Discovery;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Clients.Jmri;

public class JmriProxy : DccClientBase, IDccClient {
    private readonly Profile      _profile;
    private          JmriClient?  _client;
    public           JmriSettings _settings;

    public JmriProxy(Profile profile, IDccClientSettings clientSettings) : base(profile) {
        _profile = profile;
        _settings = clientSettings as JmriSettings ?? throw new InvalidCastException("Incorrect Settings Type provided for Jmri");
    }

    public static List<DccClientCapability> Capabilities => [DccClientCapability.Turnouts, DccClientCapability.Routes, DccClientCapability.Lights, DccClientCapability.Blocks];
    public DccClientType Type => DccClientType.Jmri;

    #region Connect and Disconnect Methods
    /// <summary>
    ///     Connects to the JMRI Server.
    /// </summary>
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
            if (result.Value is JmriSettings jmriSettings) {
                _settings.Address = jmriSettings.Address;
                _settings.Port = jmriSettings.Port;
            }
        }

        _profile?.RefreshAll();
        try {
            _client = new JmriClient(_settings.Address, _settings.Port, _settings.PollingInterval);
            await _client.ConnectAsync();
            if (_client is null || _client.ConnectionState == ConnectionStateEnum.Disconnected) throw new ApplicationException("Unable to connect to JMRI server.");

            _client.ConnectionStateChanged += ClientOnConnectionStateChanged;
            _client.TurnoutChanged += ClientOnTurnoutChanged;
            _client.BlockChanged += ClientOnBlockChanged;
            _client.LightChanged += ClientOnLightChanged;
            _client.SensorChanged += ClientOnSensorChanged;
            _client.RouteChanged += ClientOnRouteChanged;
            _client.SignalChanged += ClientOnSignalChanged;

            Status = DccClientStatus.Connected;
            OnClientMessage("Connected to Jmri");
            return Result.Ok();
        } catch (Exception ex) {
            Status = DccClientStatus.Disconnected;
            OnClientMessage(ex.Message);
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    ///     Disconnects from the JMRI Server
    /// </summary>
    public async Task<IResult> DisconnectAsync() {
        Status = DccClientStatus.Disconnected;
        OnClientMessage("Disconnected from Jmri");
        if (_client is { }) {
            _client.ConnectionStateChanged -= ClientOnConnectionStateChanged;
            _client.TurnoutChanged -= ClientOnTurnoutChanged;
            _client.BlockChanged -= ClientOnBlockChanged;
            _client.LightChanged -= ClientOnLightChanged;
            _client.SensorChanged -= ClientOnSensorChanged;
            _client.RouteChanged -= ClientOnRouteChanged;
            _client.SignalChanged -= ClientOnSignalChanged;
            _client.Dispose();
            _client = null;
        }
        return Result.Ok();
    }

    /// <summary>
    ///     Find any servers on the network and attaches to the first one it finds
    ///     or returns a Result.Failed if it could not find any server.
    /// </summary>
    public async Task<IResult> SetAutomaticSettingsAsync() {
        var settings = await GetAutomaticConnectionDetailsAsync();
        if (settings.IsSuccess) {
            if (settings.Value is JmriSettings jmriSettings) {
                _settings.Address = jmriSettings.Address;
                _settings.Port = jmriSettings.Port;
            } else {
                return Result.Fail("Unable to find any JMRI servers. ");
            }
        }
        return Result.Ok();
    }

    /// <summary>
    ///     Forces a refresh of the data from the Jmri server.
    /// </summary>
    public async Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null) {
        if (_client is { }) await _client.ResetUpdates();
        return Result.Ok();
    }

    /// <summary>
    ///     This method attempts to connect to the Jmri server and then disconnects.
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

    #region Sender Methods
    public async Task<IResult> SendTurnoutCmdAsync(Turnout turnout, bool thrown) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to JMRI server");
        if (string.IsNullOrEmpty(turnout.Id)) return Result.Fail("Invalid Turnout Id provided.");
        try {
            await _client.SetTurnoutStateAsync(turnout.Id, thrown);
            OnClientMessage($"Setting turnout {turnout.Name}({turnout.Id}) to {(thrown ? "THROWN" : "CLOSED")}", DccClientOperation.Turnout, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, "Failed to send turnout command to JMRI server");
        }
    }

    public async Task<IResult> SendRouteCmdAsync(Route route, bool active) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to JMRI server");
        if (string.IsNullOrEmpty(route.Id)) return Result.Fail("Invalid Route Id provided.");
        try {
            await _client.SetRouteStateAsync(route.Id, active);
            OnClientMessage($"Setting route {route.Name}({route.Id}) to {(active ? "ACTIVE" : "INACTIVE")}", DccClientOperation.Route, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, "Failed to send route command to JMRI server");
        }
    }

    public async Task<IResult> SendSignalCmdAsync(Signal signal, SignalAspectEnum aspect) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to JMRI server");
        if (string.IsNullOrEmpty(signal.Id)) return Result.Fail("Invalid Signal Id provided.");
        try {
            await _client.SetSignalAppearanceAsync(signal.Id, aspect.ToString());
            OnClientMessage($"Setting signal {signal.Name}({signal.Id}) to {aspect.ToString()}", DccClientOperation.Signal, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, "Failed to send signal command to JMRI server");
        }
    }

    public async Task<IResult> SendLightCmdAsync(Light light, bool isActive) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to JMRI server");
        if (string.IsNullOrEmpty(light.Id)) return Result.Fail("Invalid Light Id provided.");
        try {
            await _client.SetLightStateAsync(light.Id, isActive);
            OnClientMessage($"Setting light {light.Name}({light.Id}) to {(isActive ? "ON" : "OFF")}", DccClientOperation.Light, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, "Failed to send light command to JMRI server");
        }
    }

    public async Task<IResult> SendBlockCmdAsync(Block block, bool isOccupied) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to JMRI server");
        if (string.IsNullOrEmpty(block.Id)) return Result.Fail("Invalid Block Id provided.");
        try {
            await _client.SetBlockAllocatedAsync(block.Id, isOccupied);
            OnClientMessage($"Setting block {block.Name}({block.Id}) to {(isOccupied ? "OCCUPIED" : "FREE")}", DccClientOperation.Block, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, "Failed to send block command to JMRI server");
        }
    }

    public async Task<IResult> SendSensorCmdAsync(Sensor sensor, bool isOccupied) {
        if (Status != DccClientStatus.Connected || _client is null) return Result.Fail("Not connected to JMRI server");
        if (string.IsNullOrEmpty(sensor.Id)) return Result.Fail("Invalid Sensor Id provided.");
        try {
            await _client.SetSensorStateAsync(sensor.Id, isOccupied);
            OnClientMessage($"Setting sensor {sensor.Name}({sensor.Id}) to {(isOccupied ? "OCCUPIED" : "FREE")}", DccClientOperation.Sensor, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, "Failed to send sensor command to JMRI server");
        }
    }
    #endregion

    #region Respond to Data Events
    private void ClientOnSignalChanged(object? sender, JmriSignalEventArgs e) => UpdateSignal(e.Name, e.UserName, SignalAspectEnum.Off);

    private void ClientOnRouteChanged(object? sender, JmriRouteEventArgs e) => UpdateRoute(e.Name, e.UserName, e.State == 2 ? RouteStateEnum.Active : RouteStateEnum.Inactive);

    private void ClientOnSensorChanged(object? sender, JmriSensorEventArgs e) => UpdateSensor(e.Name, e.UserName, e.State == 2);

    private void ClientOnLightChanged(object? sender, JmriLightEventArgs e) => UpdateLight(e.Name, e.UserName, e.State == 2);

    private void ClientOnBlockChanged(object? sender, JmriBlockEventArgs e) => UpdateBlock(e.Name, e.UserName, e.State == 2, e.SensorName);

    private void ClientOnTurnoutChanged(object? sender, JmriTurnoutEventArgs e) => UpdateTurnout(e.Name, e.UserName, e.State == 2 ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown);

    private void ClientOnConnectionStateChanged(object? sender, JmriConnectionChangedEventArgs e) {
        Status = e.IsConnected ? DccClientStatus.Connected : DccClientStatus.Disconnected;
        OnClientMessage();
    }
    #endregion

    #region Discover Methods
    public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
        JmriSettings settings = new();

        var findServers = await FindAvailableServicesAsync();
        if (findServers.IsFailure) return Result<IDccClientSettings?>.Fail("No available servers could be found.");

        var firstServer = findServers?.Value?.FirstOrDefault();
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
            return Result<List<DiscoveredService>>.Fail(ex, "Unable to find a Jmri server.");
        }
        return Result<List<DiscoveredService>>.Fail("Unable to find a Jmri server.");
    }
    #endregion
}