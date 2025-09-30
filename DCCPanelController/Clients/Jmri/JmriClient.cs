// using System.Collections.Concurrent;
// using System.Diagnostics;
// using System.Net.WebSockets;
// using System.Runtime.CompilerServices;
// using System.Text;
// using System.Text.Json;
// using DCCPanelController.Clients.Jmri.Events;
// using DCCPanelController.Clients.Jmri.Helpers;
//
// namespace DCCPanelController.Clients.Jmri;
//
// public class JmriClient : IDisposable {
//     private const int DefaultConnectTimeoutMs   = 10000; // 10s
//     private const int DefaultHandshakeTimeoutMs = 8000;  // 8s
//     private const int MinimumRefreshMs          = 250;
//
//     private readonly ConcurrentDictionary<string, JmriBlockEventArgs>   _blocks   = new();
//     private readonly ConcurrentDictionary<string, JmriRouteEventArgs>   _routes   = new();
//     private readonly ConcurrentDictionary<string, JmriSensorEventArgs>  _sensors  = new();
//     private readonly ConcurrentDictionary<string, JmriLightEventArgs>   _lights   = new();
//     private readonly ConcurrentDictionary<string, JmriSignalEventArgs>  _signals  = new();
//     private readonly ConcurrentDictionary<string, JmriTurnoutEventArgs> _turnouts = new();
//
//     private readonly int    _reconnectDelayMs;
//     private readonly int    _refreshMs;
//     private readonly string _serverUrl;
//
//     // Collections to store current-state
//     private readonly Lock                        _connectionLock = new();
//     private          CancellationTokenSource?    _cancellationTokenSource;
//     private          Task?                       _connectionTask;
//     private          TaskCompletionSource<bool>? _handshakeCompletion;
//
//     private Timer? _heartbeatTimer;
//     private bool   _isDisposed;
//
//     private bool   _isHandshakeComplete;
//     private Timer? _refreshTimer;
//
//     private ClientWebSocket? _webSocket;
//
//     public JmriClient(string serverHost = "localhost", int serverPort = 12080, double refreshMs = 5.0, int reconnectDelayMs = 5000) : this(serverHost, serverPort, (int)(refreshMs * 1000), reconnectDelayMs) { }
//
//     public JmriClient(string serverHost = "localhost", int serverPort = 12080, int refreshMs = 1000, int reconnectDelayMs = 5000) {
//         _serverUrl = $"ws://{serverHost}:{serverPort}/json/";
//         _reconnectDelayMs = reconnectDelayMs;
//         _refreshMs = int.Max(refreshMs, MinimumRefreshMs);
//     }
//
//     // Public accessors for current-state
//     public IReadOnlyDictionary<string, JmriTurnoutEventArgs> Turnouts => _turnouts;
//     public IReadOnlyDictionary<string, JmriSignalEventArgs> Signals => _signals;
//     public IReadOnlyDictionary<string, JmriRouteEventArgs> Routes => _routes;
//     public IReadOnlyDictionary<string, JmriSensorEventArgs> Sensors => _sensors;
//     public IReadOnlyDictionary<string, JmriBlockEventArgs> Blocks => _blocks;
//     public IReadOnlyDictionary<string, JmriLightEventArgs> Lights => _lights;
//
//     public ConnectionStateEnum ConnectionState { get; private set; }
//
//     public void Dispose() {
//         if (!_isDisposed) {
//             _cancellationTokenSource?.Cancel();
//             _webSocket?.Dispose();
//             _cancellationTokenSource?.Dispose();
//             _isDisposed = true;
//         }
//     }
//
//     // Events
//     public event EventHandler<JmriHandshakeEventArgs>? ConnectionEstablished;
//     public event EventHandler<JmriConnectionChangedEventArgs>? ConnectionStateChanged;
//     public event EventHandler<JmriTurnoutEventArgs>? TurnoutChanged;
//     public event EventHandler<JmriSignalEventArgs>? SignalChanged;
//     public event EventHandler<JmriRouteEventArgs>? RouteChanged;
//     public event EventHandler<JmriSensorEventArgs>? SensorChanged;
//     public event EventHandler<JmriLightEventArgs>? LightChanged;
//     public event EventHandler<JmriBlockEventArgs>? BlockChanged;
//
//     public Task ConnectAsync() {
//         Debug.WriteLine("JMRI CLIENT: ConnectAsync");
//         lock (_connectionLock) {
//             if (_connectionTask is { IsCompleted: false }) return Task.CompletedTask;
//             _cancellationTokenSource?.Cancel();
//             _cancellationTokenSource = new CancellationTokenSource();
//             _connectionTask = MaintainConnectionAsync(_cancellationTokenSource.Token);
//         }
//         return Task.CompletedTask;
//     }
//
//     public async Task DisconnectAsync() {
//         Debug.WriteLine("JMRI CLIENT: DisconnectAsync");
//         StopHeartbeat();
//         StopRefreshTimer();
//         ConnectionState = ConnectionStateEnum.Disconnected;
//
//         // Send a goodbye message to the server
//         await SendCommandAsync(JmriHandshakeEventArgs.GoodbyeMessage);
//
//         if (_cancellationTokenSource is not null) {
//             await _cancellationTokenSource.CancelAsync();
//         }
//         if (_webSocket?.State == WebSocketState.Open) {
//             try {
//                 await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
//             } catch { /* ignore errors as we are closing */ }
//         }
//
//         if (_connectionTask != null) {
//             try {
//                 await _connectionTask.WaitAsync(TimeSpan.FromSeconds(5));
//             } catch (TimeoutException) {
//                 Debug.WriteLine("DisconnectAsync: Connection task didn't complete within timeout");
//             }
//         }
//         Debug.WriteLine("JMRI CLIENT: Disconnected Completely");
//         OnConnectionStateChanged(ConnectionState, "Disconnected from JMRI server.");
//     }
//
//     private async Task MaintainConnectionAsync(CancellationToken cancellationToken) {
//         Debug.WriteLine("JMRI CLIENT: MaintainConnectionAsync");
//         while (!cancellationToken.IsCancellationRequested) {
//             try {
//                 await ConnectAndListenAsync(cancellationToken);
//             } catch (OperationCanceledException) {
//                 break;
//             } catch (Exception ex) when (!cancellationToken.IsCancellationRequested) {
//                 Debug.WriteLine("JMRI CLIENT: Exception in MaintainConnectionAsync: " + ex.Message);
//                 ConnectionState = ConnectionStateEnum.Disconnected;
//                 OnConnectionStateChanged(ConnectionState, $"Connection error: {ex.Message}. Reconnecting in {_reconnectDelayMs}ms...");
//                 try {
//                     await Task.Delay(_reconnectDelayMs, cancellationToken);
//                 } catch (OperationCanceledException) {
//                     break;
//                 }
//             }
//         }
//     }
//
//     private async Task ConnectAndListenAsync(CancellationToken cancellationToken) {
//         Debug.WriteLine("JMRI CLIENT: ConnectAndListenAsync");
//         ConnectionState = ConnectionStateEnum.Initialising;
//         OnConnectionStateChanged(ConnectionState, "Attempting connection...");
//
//         try {
//             _webSocket?.Dispose();
//             _webSocket = new ClientWebSocket();
//             _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);
//
//             _isHandshakeComplete = false;
//             _handshakeCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
//
//             var uri = new Uri(_serverUrl);
//             Debug.WriteLine("JMRI CLIENT: Connecting to " + uri);
//
//             // CONNECT with timeout
//             using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
//                 cts.CancelAfter(DefaultConnectTimeoutMs);
//                 Debug.WriteLine("JMRI CLIENT: Attempting to connect to " + uri);
//                 await _webSocket.ConnectAsync(uri, cts.Token);
//             }
//
//             Debug.WriteLine("JMRI CLIENT: Connected");
//             ConnectionState = ConnectionStateEnum.Connected;
//             OnConnectionStateChanged(ConnectionState, "Connected; waiting for handshake");
//
//             // Start listener first so it can catch the hello immediately
//             var listenTask = ListenForMessagesAsync(cancellationToken);
//
//             // HANDSHAKE with timeout
//             Debug.WriteLine("JMRI CLIENT: Handshake started");
//             using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
//                 var timeout = Task.Delay(DefaultHandshakeTimeoutMs, cts.Token);
//                 var completed = await Task.WhenAny(_handshakeCompletion.Task, timeout);
//                 if (completed != _handshakeCompletion.Task || !_isHandshakeComplete) {
//                     Debug.WriteLine("JMRI CLIENT: Handshake timed out");
//                     throw new TimeoutException("JMRI handshake timed out");
//                 }
//             }
//
//             Debug.WriteLine("JMRI CLIENT: Handshake completed");
//             OnConnectionStateChanged(ConnectionState, "Handshake complete; subscribing");
//             await SubscribeToUpdatesAsync();
//             StartRefreshTimer(_refreshMs);
//
//             // Wait for listener to finish; if it exits due to close/error, bubble it up
//             Debug.WriteLine("JMRI CLIENT: Waiting for Listen Tasks");
//             await listenTask;
//             Debug.WriteLine("JMRI CLIENT: Listen task completed/stopped");
//             throw new WebSocketException("Listener stopped unexpectedly");
//         } catch (OperationCanceledException) {
//             throw; // let outer loop exit on cancellation
//         } catch (Exception ex) {
//             Debug.WriteLine("JMRI CLIENT: Connect and Listen Exception: " + ex.Message);
//             StopHeartbeat();
//             StopRefreshTimer();
//             try {
//                 _webSocket?.Abort();
//             } catch { /* ignore */ }
//             try {
//                 _webSocket?.Dispose();
//             } catch { /* ignore */ }
//
//             ConnectionState = ConnectionStateEnum.Disconnected;
//             OnConnectionStateChanged(ConnectionState, $"Unable to connect: {ex.Message}");
//             throw;
//         }
//     }
//
//     public Task ResetUpdates() {
//         // To reset all data from JMRI, we clear the caches which will force the 
//         // subscription to rebuild and event all data as changed data
//         // -----------------------------------------------------------------------
//         _turnouts.Clear();
//         _signals.Clear();
//         _routes.Clear();
//         _sensors.Clear();
//         _blocks.Clear();
//         _lights.Clear();
//         return Task.CompletedTask;
//     }
//
//     private async Task SubscribeToUpdatesAsync() {
//         var subscriptions = new[] {
//             new { type = "turnout", method = "list" },
//             new { type = "signalHead", method = "list" },
//             new { type = "route", method = "list" },
//             new { type = "sensor", method = "list" },
//             new { type = "light", method = "list" },
//             new { type = "block", method = "list" }
//         };
//
//         foreach (var subscription in subscriptions) {
//             await SendCommandAsync(JsonSerializer.Serialize(subscription));
//         }
//     }
//
//     private async Task ListenForMessagesAsync(CancellationToken cancellationToken) {
//         var buffer = new byte[4096];
//         var messageBuffer = new StringBuilder();
//
//         while (_webSocket is { State: WebSocketState.Open } &&
//                !cancellationToken.IsCancellationRequested &&
//                ConnectionState != ConnectionStateEnum.Disconnected) {
//             WebSocketReceiveResult result;
//             try {
//                 result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
//             } catch (OperationCanceledException) {
//                 Debug.WriteLine("JMRI CLIENT: ListenForMessagesAsync timed out");
//                 throw;
//             } catch (Exception ex) {
//                 Debug.WriteLine("JMRI CLIENT:  ListenForMessagesAsync Exception: " + ex.Message);
//                 throw new WebSocketException($"Receive failed: {ex.Message}", ex);
//             }
//
//             if (result.MessageType == WebSocketMessageType.Text) {
//                 var chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
//                 messageBuffer.Append(chunk);
//
//                 if (result.EndOfMessage) {
//                     ProcessMessage(messageBuffer.ToString());
//                     messageBuffer.Clear();
//                 }
//             } else if (result.MessageType == WebSocketMessageType.Close) {
//                 Debug.WriteLine("JMRI CLIENT: Server Closed WebSocket");
//                 OnConnectionStateChanged(ConnectionStateEnum.Disconnected, $"Server closed: {result.CloseStatus} {result.CloseStatusDescription}");
//                 throw new WebSocketException($"Closed: {result.CloseStatus} {result.CloseStatusDescription}");
//             } else {
//                 Debug.WriteLine("JMRI CLIENT: Got something we can't process: " + result.MessageType);
//             }
//         }
//     }
//
//     private void ProcessMessage(string jsonMessage) {
//         try {
//             using var document = JsonDocument.Parse(jsonMessage);
//             var root = document.RootElement;
//
//             // Sometimes the items come back as a single object
//             // and at other times as an array of objects. Mostly
//             // the array is still a single item in the array, but
//             // this splits the array and processes each item
//             // --------------------------------------------------------
//
//             var items = GetJsonItems(root);
//             foreach (var item in items) {
//                 if (!item.TryGetProperty("type", out var typeElement)) continue;
//                 var type = typeElement.GetString();
//                 if (string.IsNullOrEmpty(type)) continue;
//                 var isValidMessage = type switch {
//                     "turnout"    => JmriTurnoutEventArgs.ProcessMessage(item, _turnouts, TurnoutChanged),
//                     "signalHead" => JmriSignalEventArgs.ProcessMessage(item, _signals, SignalChanged),
//                     "route"      => JmriRouteEventArgs.ProcessMessage(item, _routes, RouteChanged),
//                     "sensor"     => JmriSensorEventArgs.ProcessMessage(item, _sensors, SensorChanged),
//                     "block"      => JmriBlockEventArgs.ProcessMessage(item, _blocks, BlockChanged),
//                     "light"      => JmriLightEventArgs.ProcessMessage(item, _lights, LightChanged),
//                     "goodbye"    => ProcessGoodbyeMessage(item),
//                     "hello"      => ProcessHelloMessage(item),
//                     "error"      => ProcessErrorMessage(item),
//                     "pong"       => true,
//                     _            => false
//                 };
//                 if (!isValidMessage) {
//                     Debug.WriteLine($"Invalid JSON Data from JMRI: {type} => {root}");
//                 }
//             }
//         } catch (Exception ex) {
//             OnConnectionStateChanged(ConnectionState, $"Message processing error: {ex.Message}");
//         }
//     }
//
//     private static IEnumerable<JsonElement> GetJsonItems(JsonElement root) {
//         var items = root.ValueKind switch {
//             JsonValueKind.Array  => root.EnumerateArray().ToList(),
//             JsonValueKind.Object => [root],
//             _                    => []
//         };
//         return items;
//     }
//
//     private bool ProcessGoodbyeMessage(JsonElement root) {
//         OnConnectionStateChanged(ConnectionStateEnum.Disconnecting, "Goodbye from JMRI; closing.");
//         _ = Task.Run(async () => {
//             try {
//                 StopHeartbeat();
//                 StopRefreshTimer();
//                 if (_webSocket?.State == WebSocketState.Open)
//                     await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server said goodbye", CancellationToken.None);
//             } catch { /* ignore */
//             }
//         });
//         return true;
//     }
//
//     private bool ProcessHelloMessage(JsonElement root) {
//         if (JmriHandshakeEventArgs.Create(root) is { } hello) {
//             ConnectionEstablished?.Invoke(this, hello);
//             _isHandshakeComplete = true;
//             _handshakeCompletion?.SetResult(true);
//             StartHeartbeat(hello.Heartbeat);
//             OnConnectionStateChanged(ConnectionStateEnum.Connected, "Connection established.");
//             return true;
//         }
//         return false;
//     }
//
//     private bool ProcessErrorMessage(JsonElement root) {
//         if (!root.TryGetProperty("data", out var dataElement)) return false;
//         var code = dataElement.GetIntProperty("code");
//         var message = dataElement.GetStringProperty("message");
//         OnConnectionStateChanged(ConnectionState, $"JMRI Raised an error: {code} => {message}");
//         return true;
//     }
//
//     private void StartHeartbeat(int heartbeatInterval) {
//         _heartbeatTimer?.Dispose();
//         if (heartbeatInterval > 0) {
//             _heartbeatTimer = new Timer(SendHeartbeat, null, heartbeatInterval, heartbeatInterval);
//         }
//     }
//
//     private void StopHeartbeat() {
//         _heartbeatTimer?.Dispose();
//         _heartbeatTimer = null;
//     }
//
//     private async void SendHeartbeat(object? state) {
//         try {
//             if (ConnectionState == ConnectionStateEnum.Connected) {
//                 await SendCommandAsync(JsonSerializer.Serialize(new { type = "ping" }));
//             }
//         } catch (Exception ex) {
//             Debug.WriteLine($"Could not send heartbeat: {ex.Message}");
//         }
//     }
//
//     private void StartRefreshTimer(int refreshInterval) {
//         _refreshTimer?.Dispose();
//         if (refreshInterval > 0) {
//             _refreshTimer = new Timer(SendRefreshCommand, null, refreshInterval, refreshInterval);
//         }
//     }
//
//     private void StopRefreshTimer() {
//         _refreshTimer?.Dispose();
//         _refreshTimer = null;
//     }
//
//     private async void SendRefreshCommand(object? state) {
//         try {
//             if (ConnectionState == ConnectionStateEnum.Connected) {
//                 await SubscribeToUpdatesAsync();
//             }
//         } catch (Exception ex) {
//             Debug.WriteLine($"Could not send refresh command: {ex.Message}");
//         }
//     }
//
//     public Task SetTurnoutStateAsync(string turnoutID, bool thrown) {
//         var command = BuildJmriMessage("turnout", "post", new Dictionary<string, object> {
//             { "type", "turnout" },
//             { "name", turnoutID },
//             { "state", thrown ? 4 : 2 }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public Task SetSignalAppearanceAsync(string signalID, string appearance) {
//         var command = BuildJmriMessage("signalMast", "post", new Dictionary<string, object> {
//             { "name", signalID },
//             { "set", appearance }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public Task SetRouteStateAsync(string routeID, bool active) {
//         var command = BuildJmriMessage("route", "post", new Dictionary<string, object> {
//             { "type", "route" },
//             { "name", routeID },
//             { "state", active ? 2 : 4 }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public Task SetSensorStateAsync(string sensorID, bool active) {
//         var command = BuildJmriMessage("sensor", "post", new Dictionary<string, object> {
//             { "type", "sensor" },
//             { "name", sensorID },
//             { "state", active ? 2 : 4 }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public Task SetLightStateAsync(string lightID, bool active) {
//         var command = BuildJmriMessage("light", "post", new Dictionary<string, object> {
//             { "type", "light" },
//             { "name", lightID },
//             { "state", active ? 2 : 4 }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public Task SetBlockValueAsync(string blockID, string value) {
//         var command = BuildJmriMessage("block", "post", new Dictionary<string, object> {
//             { "type", "block" },
//             { "name", blockID },
//             { "state", value }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public Task SetBlockAllocatedAsync(string blockID, bool allocated) {
//         var command = BuildJmriMessage("block", "post", new Dictionary<string, object> {
//             { "type", "block" },
//             { "name", blockID },
//             { "state", allocated ? 2 : 4 }
//         });
//         return SendCommandAsync(command);
//     }
//
//     public static string BuildJmriMessage(string type, string? method, Dictionary<string, object>? parameters) {
//         var message = new Dictionary<string, object?> { ["type"] = type };
//         if (method is not null) message["method"] = method;
//         if (parameters is { Count: > 0 }) {
//             message["data"] = parameters;
//         }
//         return JsonSerializer.Serialize(message);
//     }
//
//     private Task SendCommandAsync(string? jsonSerialisedCommand) {
//         if (_isHandshakeComplete && !string.IsNullOrEmpty(jsonSerialisedCommand)) {
//             return SendMessageAsync(jsonSerialisedCommand);
//         }
//         return Task.CompletedTask;
//     }
//
//     private async Task SendMessageAsync(string? message) {
//         if (!string.IsNullOrEmpty(message) && _webSocket?.State == WebSocketState.Open) {
//             var bytes = Encoding.UTF8.GetBytes(message);
//             using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
//             await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
//         }
//     }
//
//     private void OnConnectionStateChanged(ConnectionStateEnum state, string message, [CallerMemberName] string member = "unknown", [CallerLineNumber] long line = 0) {
//         ConnectionStateChanged?.Invoke(this, new JmriConnectionChangedEventArgs {
//             ConnectionState = state,
//             Message = message,
//             CallerDetails = $"{member}@{line}"
//         });
//     }
// }