using DCCClients.Common;
using DCCClients.Events;
using DCCClients.Interfaces;
using DCCClients.WiThrottle.Client;
using DCCClients.WiThrottle.Client.Commands;
using DCCClients.WiThrottle.Client.Events;
using DCCClients.WiThrottle.ServiceHelper;

namespace DCCClients;

public class DccWiThrottleClient : DccClient, IDccClient {
    private readonly WithrottleSettings? _settings;
    private Client? _client;

    public DccWiThrottleClient(IDccSettings? settings) {
        _settings = settings as WithrottleSettings ?? throw new ArgumentException("Invalid settings provided.");
    }

    public bool IsConnected => _client is not null && _client.IsRunning;

    /// <summary>
    ///     Establishes a connection to the WiThrottle server using the provided settings.
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
            _client = CreateClient(_settings);
            _client.ConnectionEvent += OnOnConnectionEvent;
            _client.DataEvent += ProcessClientEvent;
            return await _client.ConnectAsync();
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to connect to the Withrottle server.").CausedBy(ex));
        }
    }

    /// <summary>
    /// Creates a new Client instance. Can be overridden in tests to provide a mock.
    /// </summary>
    protected virtual Client CreateClient(WithrottleSettings settings)
    {
        return new Client(settings);
    }

    /// <summary>
    ///     Attempts to reconnect to the WiThrottle server using the existing client connection.
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
    ///     Disconnects from the service and releases related resources.
    /// </summary>
    /// <returns>A result indicating success or failure of the disconnect operation.</returns>
    public IResult Disconnect() {
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
        return Result.Ok();
    }

    public IResult SendCmd(string message) {
        try {
            Console.WriteLine($"Sending command: {message}");
            _client?.SendMessage(message);
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        return Result.Ok();
    }

    public IResult SendTurnoutCmd(string dccAddress, bool isThrown) {
        try {
            Console.WriteLine($"Sending turnout command: {dccAddress} - {isThrown}");
            _client?.SendMessage(new TurnoutCommand(dccAddress, isThrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed));
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        return Result.Ok();
    }

    public IResult SendRouteCmd(string dccAddress, bool isActive) {
        try {
            Console.WriteLine($"Sending route command: {dccAddress} - {isActive}");
            _client?.SendMessage(new RouteCommand(dccAddress));
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to send command to the Withrottle server.").CausedBy(ex));
        }
        return Result.Ok();
    }

    public IResult SendSignalCmd(string dccAddress, SignalAspectEnum aspect) {
        return Result.Fail("Withrottle does not support signal commands.");
    }

    private async Task<IResult<ServiceInfo?>> FindServices() {
        var services = await ServiceFinder.FindServices("_withrottle._tcp");
        if (services.Count == 0) return Result<ServiceInfo>.Fail(new Error("Unable to find a WiThrottle server."));
        return Result<ServiceInfo>.Ok(services[0]);
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
            OnMessageReceived(new DccMessageArgs("Turnout", turnout.ToString()));
            OnTurnoutMsgReceived(new DccTurnoutArgs(turnout.SystemName, turnout.UserName, turnout.StateEnum == TurnoutStateEnum.Thrown));
            break;

        case RouteEvent route:
            OnMessageReceived(new DccMessageArgs("Route", route.ToString()));
            OnRouteMsgReceived(new DccRouteArgs(route.SystemName, route.UserName, route.StateEnum == RouteStateEnum.Active));
            break;

        default:
            OnMessageReceived(new DccMessageArgs(clientEvent.GetType().Name, clientEvent.ToString()));
            break;
        }
    }

    protected virtual void OnOnConnectionEvent(IClientEvent clientEvent) {
        Console.WriteLine($"Connection error: {clientEvent.GetType().Name} - {clientEvent}");
        OnConnectionError(new DccErrorArgs(clientEvent.ToString(), _client?.IsRunning ?? false));
    }
}
