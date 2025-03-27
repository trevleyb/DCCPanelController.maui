using DCCClients.Common;
using DCCClients.Events;
using DCCClients.Interfaces;
using DCCClients.JMRI.EventArgs;
using DCCClients.WiThrottle.Client;
using DCCClients.WiThrottle.Client.Commands;
using DCCClients.WiThrottle.Client.Events;
using DCCClients.WiThrottle.ServiceHelper;

namespace DCCClients;

public class DccWiThrottleClient : DccClient, IDccClient {
    private Client? _client;
    private WithrottleSettings? _settings;

    public DccWiThrottleClient(IDccSettings? settings) {
        _settings = settings as WithrottleSettings ?? throw new ArgumentException("Invalid settings provided.");
    }

    /// <summary>
    /// Establishes a connection to the WiThrottle server using the provided settings.
    /// </summary>
    /// <param name="settings">The settings required to configure the connection, such as server address and port.</param>
    /// <returns>Returns a result indicating the success or failure of the connection attempt.</returns>
    public async Task<IResult> ConnectAsync() {
        ArgumentNullException.ThrowIfNull(_settings);
        if (_client is not null && _client.IsRunning) return Result.Ok("A connection is already established.");
        
        // Validate that we have an address for the WiService. If not, try and find one. 
        // ------------------------------------------------------------------------------------------
        if (string.IsNullOrEmpty(_settings.Address) || _settings.Port == 0 || _settings.Address == "0.0.0.0") {
            //var service = await FindServices();
            //if (service.IsFailure) return service; 
            //_settings.Address = service?.Value?.WithrottleSettings?.Address ?? string.Empty;
            //_settings.Port = service?.Value?.WithrottleSettings?.Port ?? 12090;
            _settings.Address = "192.168.68.54";
            _settings.Port = 12090;
        }
        
        // Connect to the service
        // --------------------------
        try {
            _client = new Client(_settings);
            _client.ConnectionError += OnOnConnectionError;
            _client.ConnectionEvent += ProcessClientEvent;
            return await _client.ConnectAsync();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to connect to the Withrottle server.").CausedBy(ex));
        }
    }

    private async Task<IResult<ServiceInfo?>> FindServices() {
        var services = await ServiceFinder.FindServices("_withrottle._tcp");
        if (services.Count == 0) return Result<ServiceInfo>.Fail(new Error("Unable to find a WiThrottle server."));
        return Result<ServiceInfo>.Ok(services[0]);
    }

    /// <summary>
    /// Attempts to reconnect to the WiThrottle server using the existing client connection.
    /// </summary>
    /// <returns>Returns a result indicating the success or failure of the reconnection attempt.</returns>
    public async Task<IResult> ReconnectAsync() {
        try {
            if (_client is not null) return await _client.ConnectAsync();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to reconnect to the Withrottle server.").CausedBy(ex));
        }
        return Result.Fail(new Error("Unable to reconnect as no connection was established."));
    }

    /// <summary>
    /// Disconnects from the service and releases related resources.
    /// </summary>
    /// <returns>A result indicating success or failure of the disconnect operation.</returns>
    public IResult Disconnect() {
        try {
            _client?.Disconnect();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to disconnect from the Withrottle server.").CausedBy(ex));
        } finally {
            if (_client is not null) {
                _client.ConnectionError -= OnOnConnectionError;
                _client.ConnectionEvent -= ProcessClientEvent;
            }
        }
        _client = null;
        return Result.Ok();
    }

    public IResult SendCmd(string message) {
        try {
            _client?.SendMessage(message);
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        return Result.Ok();
    }

    public IResult SendTurnoutCmd(string dccAddress, bool isThrown) {
        try {
            _client?.SendMessage(new TurnoutCommand(dccAddress, isThrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed));
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        return Result.Ok();
    }

    public IResult SendRouteCmd(string dccAddress, bool isActive) {
        try {
            _client?.SendMessage(new RouteCommand(dccAddress));
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        return Result.Ok();
    }

    public IResult SendSignalCmd(string dccAddress, SignalAspectEnum aspect) {
        return Result.Fail("Withrottle does not support signal commands.");
    }

    /// <summary>
    /// Processes a client event received from the WiThrottle client and triggers the appropriate actions based on the event type.
    /// </summary>
    /// <param name="clientEvent">The client event to be processed, which can include various types such as message, turnout, route, or roster events.</param>
    private void ProcessClientEvent(IClientEvent clientEvent) {
        switch (clientEvent) {
        case MessageEvent message:
            Console.WriteLine($"Message received: {message.Value}");
            OnMessageReceived(new DccMessageArgs(message.Type, message.Value));
            break;

        case TurnoutEvent turnout:
            Console.WriteLine($"Turnout event received: {turnout.SystemName} - {turnout.UserName} - {turnout.StateEnum}");
            OnTurnoutMsgReceived(new DccTurnoutArgs(turnout.SystemName, turnout.UserName, turnout.StateEnum == TurnoutStateEnum.Thrown));
            break;

        case RouteEvent route:
            Console.WriteLine($"Route event received: {route.SystemName} - {route.UserName} - {route.StateEnum}");
            OnRouteMsgReceived(new DccRouteArgs(route.SystemName, route.UserName, route.StateEnum == RouteStateEnum.Active));
            break;

        default:
            Console.WriteLine($"Event received: {clientEvent.GetType().Name} - {clientEvent}");
            OnMessageReceived(new DccMessageArgs(clientEvent.GetType().Name, clientEvent.ToString()));
            break;
        }
    }

    protected virtual void OnOnConnectionError(string error) {
        Console.WriteLine($"Connection error: {error}");
        OnConnectionError(new DccErrorArgs(error, _client?.IsRunning ?? false));
    }
}