using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri;

public class JmriClient : IDisposable {
    private const int MinimumRefreshMs = 250;
    private readonly string _serverUrl;
    private readonly int _reconnectDelayMs;
    private readonly int _refreshMs;
    private readonly Lock _connectionLock = new();

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _connectionTask;
    private bool _isDisposed = false;

    private bool _isHandshakeComplete = false;
    private TaskCompletionSource<bool>? _handshakeCompletion;

    private Timer? _heartbeatTimer;
    private Timer? _refreshTimer;

    // Collections to store current-state
    private readonly ConcurrentDictionary<string, JmriTurnoutEventArgs> _turnouts = new();
    private readonly ConcurrentDictionary<string, JmriSignalEventArgs> _signals = new();
    private readonly ConcurrentDictionary<string, JmriRouteEventArgs> _routes = new();
    private readonly ConcurrentDictionary<string, JmriSensorEventArgs> _sensors = new();
    private readonly ConcurrentDictionary<string, JmriBlockEventArgs> _blocks = new();

    // Events
    public event EventHandler<JmriInitialisedEventArgs>? ConnectionEstablished;
    public event EventHandler<JmriConnectionChangedEventArgs>? ConnectionStateChanged;
    public event EventHandler<JmriTurnoutEventArgs>? TurnoutChanged;
    public event EventHandler<JmriSignalEventArgs>? SignalChanged;
    public event EventHandler<JmriRouteEventArgs>? RouteChanged;
    public event EventHandler<JmriSensorEventArgs>? SensorChanged;
    public event EventHandler<JmriBlockEventArgs>? BlockChanged;

    // Public accessors for current-state
    public IReadOnlyDictionary<string, JmriTurnoutEventArgs> Turnouts => _turnouts;
    public IReadOnlyDictionary<string, JmriSignalEventArgs> Signals => _signals;
    public IReadOnlyDictionary<string, JmriRouteEventArgs> Routes => _routes;
    public IReadOnlyDictionary<string, JmriSensorEventArgs> Sensors => _sensors;
    public IReadOnlyDictionary<string, JmriBlockEventArgs> Blocks => _blocks;

    public ConnectionStateEnum ConnectionState { get; private set; }

    public JmriClient(string serverHost = "localhost", int serverPort = 12080, double refreshMs = 5.0, int reconnectDelayMs = 5000) : this(serverHost, serverPort, (int)(refreshMs * 1000), reconnectDelayMs) { }

    public JmriClient(string serverHost = "localhost", int serverPort = 12080, int refreshMs = 1000, int reconnectDelayMs = 5000) {
        _serverUrl = $"ws://{serverHost}:{serverPort}/json/";
        _reconnectDelayMs = reconnectDelayMs;
        _refreshMs = int.Max(refreshMs, MinimumRefreshMs);
    }

    public async Task ConnectAsync() {
        lock (_connectionLock) {
            if (_connectionTask is { IsCompleted: false }) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _connectionTask = MaintainConnectionAsync(_cancellationTokenSource.Token);
        }
        await Task.CompletedTask;
    }

    public async Task DisconnectAsync() {
        StopHeartbeat();
        StopRefreshTimer();
        ConnectionState = ConnectionStateEnum.Disconnected;

        if (_cancellationTokenSource is not null) {
            await _cancellationTokenSource.CancelAsync();
        }
        if (_webSocket?.State == WebSocketState.Open) {
            try {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
            } catch { /* ignore errors as we are closing */
            }
        }

        if (_connectionTask != null) {
            try {
                await _connectionTask.WaitAsync(TimeSpan.FromSeconds(5));
            } catch (TimeoutException) {
                Console.WriteLine("DisconnectAsync: Connection task didn't complete within timeout");
            }
        }
        OnConnectionStateChanged(ConnectionState, "Disconnected from JMRI server.");
    }

    private async Task MaintainConnectionAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                await ConnectAndListenAsync(cancellationToken);
            } catch (OperationCanceledException) {
                // Cancellation requested - exit cleanly
                break;
            } catch (Exception ex) when (!cancellationToken.IsCancellationRequested) {
                ConnectionState = ConnectionStateEnum.Disconnected;
                OnConnectionStateChanged(ConnectionState, $"Connection error: {ex.Message}");

                try {
                    await Task.Delay(_reconnectDelayMs, cancellationToken);
                } catch (OperationCanceledException) {
                    break;
                }
            }
        }
    }

    private async Task ConnectAndListenAsync(CancellationToken cancellationToken) {
        ConnectionState = ConnectionStateEnum.Initialising;

        _webSocket?.Dispose();
        _webSocket = new ClientWebSocket();

        _isHandshakeComplete = false;
        _handshakeCompletion = new TaskCompletionSource<bool>();

        var uri = new Uri(_serverUrl);
        await _webSocket.ConnectAsync(uri, cancellationToken);

        ConnectionState = ConnectionStateEnum.Connected;
        OnConnectionStateChanged(ConnectionState, "Connected to JMRI server, waiting for handshake");

        var listenTask = ListenForMessagesAsync(cancellationToken);

        // Wait for handshake with cancellation support
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var handshakeTask = _handshakeCompletion.Task;
        var completedTask = await Task.WhenAny(handshakeTask, Task.Delay(-1, combinedCts.Token));

        if (completedTask != handshakeTask) {
            // Cancellation occurred before handshake
            cancellationToken.ThrowIfCancellationRequested();
        }

        OnConnectionStateChanged(ConnectionState, "Handshake complete, subscribing to updates");
        StartRefreshTimer(_refreshMs);

        // Now wait for the listen task to complete
        await listenTask;
    }

    private async Task SubscribeToUpdatesAsync() {
        var subscriptions = new[] {
            new { type = "turnout", method = "list" },
            new { type = "signalHead", method = "list" },
            new { type = "route", method = "list" },
            new { type = "sensor", method = "list" },
            new { type = "block", method = "list" }
        };

        foreach (var subscription in subscriptions) {
            await SendCommandAsync(JsonSerializer.Serialize(subscription));
        }
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken) {
        var buffer = new byte[4096];
        var messageBuffer = new StringBuilder();

        while (_webSocket is { State: WebSocketState.Open } && !cancellationToken.IsCancellationRequested && ConnectionState != ConnectionStateEnum.Disconnected) {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Text) {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuffer.Append(message);

                if (result.EndOfMessage) {
                    ProcessMessage(messageBuffer.ToString());
                    messageBuffer.Clear();
                }
            } else if (result.MessageType == WebSocketMessageType.Close) {
                Console.WriteLine($"WebSocket was CLOSED.");
                break;
            }
        }
    }

    private void ProcessMessage(string jsonMessage) {
        try {
            using var document = JsonDocument.Parse(jsonMessage);
            var root = document.RootElement;

            // Sometimes the items come back as a single object
            // and at other times as an array of objects. Mostly
            // the array is still a single item in the array, but
            // this splits the array and processes each item
            // --------------------------------------------------------

            var items = GetJsonItems(root);
            foreach (var item in items) {
                if (!item.TryGetProperty("type", out var typeElement)) continue;
                var type = typeElement.GetString();
                if (string.IsNullOrEmpty(type)) continue;
                var isValidMessage = type.ToLowerInvariant() switch {
                    "turnout"    => ProcessTurnoutMessage(item),
                    "signalHead" => ProcessSignalMessage(item),
                    "route"      => ProcessRouteMessage(item),
                    "sensor"     => ProcessSensorMessage(item),
                    "block"      => ProcessBlockMessage(item),
                    "hello"      => ProcessHelloMessage(item),
                    "error"      => ProcessErrorMessage(item),
                    "pong"       => ProcessPongMessage(item),
                    _            => false
                };
                if (!isValidMessage) {
                    Console.WriteLine($"Invalid JSON Data from JMRI: {type} => {root}");
                }
            }
        } catch (Exception ex) {
            OnConnectionStateChanged(ConnectionState, $"Message processing error: {ex.Message}");
        }
    }

    private static IEnumerable<JsonElement> GetJsonItems(JsonElement root) {
        var items = root.ValueKind switch {
            JsonValueKind.Array  => root.EnumerateArray().ToList(),
            JsonValueKind.Object => [root],
            _                    => []
        };
        return items;
    }

    private bool ProcessTurnoutMessage(JsonElement root) {
        if (JmriTurnoutEventArgs.Create(root) is { } turnout) {
            var existingTurnout = _turnouts.GetValueOrDefault(turnout.Name);
            _turnouts[turnout.Name] = turnout;
            if (existingTurnout == null || existingTurnout.State != turnout.State) {
                TurnoutChanged?.Invoke(this, turnout);
            }
            return true;
        }
        return false;
    }

    private bool ProcessSignalMessage(JsonElement root) {
        if (JmriSignalEventArgs.Create(root) is { } signal) {
            var existingSignal = _signals.GetValueOrDefault(signal.Name);
            _signals[signal.Name] = signal;

            if (existingSignal == null || existingSignal.Appearance != signal.Appearance ||
                existingSignal.Lit != signal.Lit || existingSignal.Held != signal.Held) {
                SignalChanged?.Invoke(this, signal);
            }
            return true;
        }
        return false;
    }

    private bool ProcessRouteMessage(JsonElement root) {
        if (JmriRouteEventArgs.Create(root) is { } route) {
            var existingRoute = _routes.GetValueOrDefault(route.Name);
            _routes[route.Name] = route;

            if (existingRoute == null || existingRoute.State != route.State) {
                RouteChanged?.Invoke(this, route);
            }
            return true;
        }
        return false;
    }

    private bool ProcessSensorMessage(JsonElement root) {
        if (JmriSensorEventArgs.Create(root) is { } sensor) {
            var existingSensor = _sensors.GetValueOrDefault(sensor.Name);
            _sensors[sensor.Name] = sensor;

            if (existingSensor == null || existingSensor.State != sensor.State) {
                SensorChanged?.Invoke(this, sensor);
            }
            return true;
        }
        return false;
    }

    private bool ProcessBlockMessage(JsonElement root) {
        if (JmriBlockEventArgs.Create(root) is { } block) {
            var existingBlock = _blocks.GetValueOrDefault(block.Name);
            _blocks[block.Name] = block;

            if (existingBlock == null || existingBlock.State != block.State ||
                existingBlock.Value != block.Value || existingBlock.Allocated != block.Allocated) {
                BlockChanged?.Invoke(this, block);
            }
            return true;
        }
        return false;
    }

    private bool ProcessHelloMessage(JsonElement root) {
        if (JmriInitialisedEventArgs.Create(root) is { } hello) {
            ConnectionEstablished?.Invoke(this, hello);
            _isHandshakeComplete = true;
            _handshakeCompletion?.SetResult(true);
            StartHeartbeat(hello.Heartbeat);
            OnConnectionStateChanged(ConnectionState, $"Connection to JMRI has been established.");
            return true;
        }
        return false;
    }

    private void StartHeartbeat(int heartbeatInterval) {
        _heartbeatTimer?.Dispose();
        if (heartbeatInterval > 0) {
            _heartbeatTimer = new Timer(SendHeartbeat, null, heartbeatInterval, heartbeatInterval);
        }
    }

    private void StopHeartbeat() {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    private async void SendHeartbeat(object? state) {
        try {
            if (ConnectionState == ConnectionStateEnum.Connected) {
                await SendCommandAsync(JsonSerializer.Serialize(new { type = "ping" }));
            }
        } catch (Exception ex) {
            Console.WriteLine($"Could not send heartbeat: {ex.Message}");
        }
    }

    private void StartRefreshTimer(int refreshInterval) {
        _refreshTimer?.Dispose();
        if (refreshInterval > 0) {
            _refreshTimer = new Timer(SendRefreshCommand, null, refreshInterval, refreshInterval);
        }
    }

    private void StopRefreshTimer() {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    private async void SendRefreshCommand(object? state) {
        try {
            if (ConnectionState == ConnectionStateEnum.Connected) {
                await SubscribeToUpdatesAsync();
            }
        } catch (Exception ex) {
            Console.WriteLine($"Could not send refresh command: {ex.Message}");
        }
    }

    private bool ProcessErrorMessage(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return false;
        var code = dataElement.GetIntProperty("code");
        var message = dataElement.GetStringProperty("message");
        Console.WriteLine($"JMRI Raised an error: {code} => {message}");
        return true;
    }

    private bool ProcessPongMessage(JsonElement root) {
        // Nothing to do for a Pong message
        return true;
    }

    public async Task SetTurnoutStateAsync(string turnoutID, bool thrown) {
        var command = BuildJmriMessage("turnout", "post", new Dictionary<string, object> {
            { "type", "turnout" },
            { "name", turnoutID },
            { "state", thrown ? 4 : 2 }
        });
        await SendCommandAsync(command);
    }

    public async Task SetSignalAppearanceAsync(string signalID, string appearance) {
        var command = BuildJmriMessage("signalMast", "post", new Dictionary<string, object> {
            { "name", signalID },
            { "set", appearance }
        });
        await SendCommandAsync(command);
    }

    public async Task SetRouteStateAsync(string routeID, bool active) {
        var command = BuildJmriMessage("route", "post", new Dictionary<string, object> {
            { "type", "route" },
            { "name", routeID },
            { "state", active ? 2 : 4 }
        });
        await SendCommandAsync(command);
    }

    public async Task SetBlockValueAsync(string blockID, string value) {
        var command = BuildJmriMessage("block", "post", new Dictionary<string, object> {
            { "type", "block" },
            { "name", blockID },
            { "action", value }
        });
        await SendCommandAsync(command);
    }

    public async Task SetBlockAllocatedAsync(string blockID, bool allocated) {
        var command = BuildJmriMessage("block", "post", new Dictionary<string, object> {
            { "type", "block" },
            { "name", blockID },
            { "state", allocated ? 2 : 4 }
        });
        await SendCommandAsync(command);
    }

    public static string BuildJmriMessage(string type, string? method, Dictionary<string, object>? parameters) {
        var message = new Dictionary<string, object?> { ["type"] = type };
        if (method is not null) message["method"] = method;
        if (parameters is { Count: > 0 }) {
            message["data"] = parameters;
        }
        return JsonSerializer.Serialize(message);
    }

    private async Task SendCommandAsync(string? jsonSerialisedCommand) {
        if (_isHandshakeComplete && !string.IsNullOrEmpty(jsonSerialisedCommand)) {
            await SendMessageAsync(jsonSerialisedCommand);
        }
    }

    private async Task SendMessageAsync(string? message) {
        if (!string.IsNullOrEmpty(message) && _webSocket?.State == WebSocketState.Open) {
            var bytes = Encoding.UTF8.GetBytes(message);
            try {
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            } catch (Exception ex) {
                Console.WriteLine($"Failed to send message to server: {ex.Message}");
                throw;
            }
        }
    }

    private void OnConnectionStateChanged(ConnectionStateEnum state, string message, [CallerMemberName] string member = "unknown", [CallerLineNumber] long line = 0) {
        ConnectionStateChanged?.Invoke(this, new JmriConnectionChangedEventArgs {
            ConnectionState = state,
            Message = message,
            CallerDetails = $"{member}@{line}"
        });
    }

    public void Dispose() {
        if (!_isDisposed) {
            _cancellationTokenSource?.Cancel();
            _webSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
            _isDisposed = true;
        }
    }
}