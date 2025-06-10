using DccClients.WiThrottle.Client;
using DccClients.WiThrottle.Client.Commands;
using DccClients.WiThrottle.Client.Events;
using DccClients.WiThrottle.ServiceHelper;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCCommon.Events;
using DCCCommon.Helpers;

namespace DccClients.WiThrottle;

public class DccWiThrottleClient : DccClientBase, IDccClient {
    private WiThrottleClientSettings? _settings;
    private Client.WithrottleClient? _client;

    public DccClientType Type => DccClientType.WiThrottle;

    public DccWiThrottleClient(IDccClientSettings? settings) {
        _settings = settings as WiThrottleClientSettings ?? throw new ArgumentException("Invalid settings provided.");
    }

    public bool IsConnected => _client is not null && _client.IsRunning;

    public async Task<IResult> ConnectAsync(IDccClientSettings? settings) {
        if (settings is not WiThrottleClientSettings withrottleSettings) return Result.Fail("Invalid settings provided.");
        _settings = withrottleSettings;
        return await ConnectAsync();
    }

    public async Task<IResult> ConnectAsync() {
        ArgumentNullException.ThrowIfNull(_settings);
        if (_client is not null && _client.IsRunning) return Result.Ok("A connection is already established.");

        // Validate that we have an address for the WiService. If not, try and find one. 
        // ------------------------------------------------------------------------------------------
        if (string.IsNullOrEmpty(_settings.Address) || _settings.Port == 0 || _settings.Address == "0.0.0.0") {
            _settings.Address = "192.168.68.54";
            _settings.Port = 12090;
        }

        // Connect to the service
        // --------------------------
        try {
            _client = CreateClient(_settings);
            _client.ConnectionEvent += OnOnConnectionEvent;
            _client.DataEvent += ProcessClientEvent;
            return await _client.ConnectAsync();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to connect to the Withrottle server.").CausedBy(ex));
        }
    }

    /// <summary>
    ///     Attempts to reconnect to the WiThrottle server using the existing client connection.
    /// </summary>
    /// <returns>Returns a result indicating the success or failure of the reconnection attempt.</returns>
    public async Task<IResult> ReconnectAsync() {
        try {
            await DisconnectAsync();
            return await ConnectAsync();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to reconnect to the Withrottle server.").CausedBy(ex));
        }
    }

    public async Task<IResult> ForceRefreshAsync(string? type = null) {
        return await ReconnectAsync();
    }

    public async Task<IResult> DisconnectAsync() {
        try {
            _client?.Disconnect();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to disconnect from the Withrottle server.").CausedBy(ex));
        } finally {
            if (_client is not null) {
                _client.ConnectionEvent -= OnOnConnectionEvent;
                _client.DataEvent -= ProcessClientEvent;
            }
        }
        _client = null;
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> ValidateConnectionAsync() {
        try {
            var result = await ConnectAsync(_settings);
            if (result.IsFailure) return result;
            await Task.Delay(1000);
            if (IsConnected) {
                await DisconnectAsync();
                return Result.Ok("Successfully connected to the server.");
            } else {
                return Result.Fail($"Unable to connect to the server. Check settings.");
            }
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to connect to the server.").CausedBy(ex));
        }
    }

    public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
        var findSettings = _settings ?? new WiThrottleClientSettings();
        
        var findServers = await FindAvailableServicesAsync();
        if (findServers.IsFailure) return Result<IDccClientSettings?>.Fail("No available servers could be found.");
        
        var firstServer = findServers?.Value?.FirstOrDefault();
        if (firstServer is null) return Result<IDccClientSettings?>.Fail("Unable to set connection automatically. No WiThrottle servers found."); 
        
        findSettings.Address = firstServer.Address.ToString();
        findSettings.Port = firstServer.Port;
        return Result<IDccClientSettings?>.Ok(findSettings);
    }
    
    public async Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() {
        try {
            var result = await DiscoverServices.SearchForServicesByTypeAsync(DccClientType.WiThrottle);
            if (result is { IsSuccess: true, Value.Count: > 0 }) return result;
        } catch (Exception ex) {
            return Result<List<DiscoveredService>>.Fail(new Error("Unable to find a WiThrottle server.").CausedBy(ex));
        }
        return Result<List<DiscoveredService>>.Fail("Unable to find a WiThrottle server.");
    }
    
    public async Task<IResult> SendCmdAsync(string message) {
        try {
            Console.WriteLine($"Sending command: {message}");
            _client?.SendMessage(message);
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> SendTurnoutCmdAsync(string turnout, bool isThrown) {
        try {
            Console.WriteLine($"Sending turnout command: {turnout} - {isThrown}");
            _client?.SendMessage(new TurnoutCommand(turnout, isThrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed));
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> SendRouteCmdAsync(string route, bool isActive) {
        try {
            Console.WriteLine($"Sending route command: {route} - {isActive}");
            _client?.SendMessage(new RouteCommand(route));
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> SendLightCmdAsync(string signal, bool isActive) {
        await Task.CompletedTask;
        return Result.Fail("Withrottle does not support light commands.");
    }

    public async Task<IResult> SendSensorCmdAsync(string sensor, bool isOccupied) {
        await Task.CompletedTask;
        return Result.Fail("Withrottle does not support sensor commands.");
    }

    public async Task<IResult> SendBlockCmdAsync(string block, bool isOccupied) {
        await Task.CompletedTask;
        return Result.Fail("Withrottle does not support block commands.");
    }

    public async Task<IResult> SendSignalCmdAsync(string signal, SignalAspectEnum aspect) {
        await Task.CompletedTask;
        return Result.Fail("Withrottle does not support signal commands.");
    }

    /// <summary>
    ///     Creates a new Client instance. Can be overridden in tests to provide a mock.
    /// </summary>
    protected virtual Client.WithrottleClient CreateClient(WiThrottleClientSettings clientSettings) {
        return new Client.WithrottleClient(clientSettings);
    }

    public async Task ForceRefresh(string? type) {
        // Force refresh for WiThrottle requires that we disconnect and re-connect
        // as the turnout and route data is ONLY sent on initialisation. 
        await ReconnectAsync();
    }

    private async Task<IResult<ServiceInfo?>> FindServices() {
        var services = await ServiceFinder.FindServices("_withrottle._tcp");
        if (services.Count == 0) return Result<ServiceInfo?>.Fail(new Error("Unable to find a WiThrottle server."));
        return Result<ServiceInfo?>.Ok(services[0]);
    }

    /// <summary>
    ///     Processes a client event received from the WiThrottle client and triggers the appropriate actions based on the
    ///     event type.
    /// </summary>
    /// <param name="clientEvent">
    ///     The client event to be processed, which can include various types such as message, turnout,
    ///     route, or roster events.
    /// </param>
    private void ProcessClientEvent(IClientEvent clientEvent) {
        switch (clientEvent) {
        case MessageEvent message:
            OnMessageReceived(new DccMessageArgs(message.Type, message.Value));
            break;

        case TurnoutEvent turnout:
            OnTurnoutMsgReceived(new DccTurnoutArgs() {
                TurnoutId = turnout.SystemName,
                Username = turnout.UserName,
                DccAddress = turnout.SystemName.ConvertToDCCAddress(),
                IsThrown = turnout.StateEnum == TurnoutStateEnum.Thrown
            });
            break;

        case RouteEvent route:
            OnRouteMsgReceived(new DccRouteArgs() {
               RouteId  = route.SystemName, 
               UserName = route.UserName, 
               IsActive = route.StateEnum == RouteStateEnum.Active
            });
            break;

        default:
            OnMessageReceived(new DccMessageArgs(clientEvent.GetType().Name, clientEvent.ToString()));
            break;
        }
    }

    protected virtual void OnOnConnectionEvent(IClientEvent clientEvent) {
        Console.WriteLine($"Connection error: {clientEvent.GetType().Name} - {clientEvent}");
        OnConnectionStateChanged(new DccStateChangedArgs(_client?.IsRunning ?? false, clientEvent.ToString()));
    }
}