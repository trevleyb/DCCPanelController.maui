using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DccClients.Jmri.EventArgs;
using DCCCommon.Client;
using DCCCommon.Common;

namespace DccClients.Jmri.Client;

public class JmriClient {
    private static readonly HttpClient HttpClient = new() {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private readonly string _jmriUrl;
    private readonly Dictionary<string, string> _previousOccupancyStates = new();
    private readonly Dictionary<string, string> _previousRouteStates = new();
    private readonly Dictionary<string, string> _previousSignalStates = new();
    private readonly Dictionary<string, string> _previousTurnoutStates = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private IWebSocket _webSocket = new JmriWebSocket();

    public JmriClient(IDccClientSettings clientSettings) {
        if (clientSettings is JmriClientSettings info) {
            _jmriUrl = info.Url.TrimEnd('/');
            var pollInterval = Convert.ToInt32(info.PollingInterval * 1000);
            PollingIntervalMs = int.Min(int.Max(500, pollInterval),10000); 
        } else throw new ArgumentException("Invalid settings provided.");
    }

    // Configuration properties
    public int ConnectionTimeoutSeconds { get; set; } = 10;
    public int MaxConnectionRetries { get; set; } = 5;
    public int PollingIntervalMs { get; set; }
    public int ReconnectionDelayMs { get; set; } = 2000;

    public Func<HttpClient> HttpClientFactory { get; set; } = () => new HttpClient();
    public Func<IWebSocket> WebSocketFactory { get; set; } = () => new JmriWebSocket();

    // Event to notify about connection status changes
    public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;

    public event EventHandler<TurnoutEventArgs>? TurnoutChanged;
    public event EventHandler<RouteEventArgs>? RouteChanged;
    public event EventHandler<OccupancyEventArgs>? OccupancyChanged;
    public event EventHandler<SignalEventArgs>? SignalChanged;

    public async Task<IResult> Open() {
        // Initialize the client and fetch initial data
        var initialised = await InitializeAsync();
        if (initialised.IsFailure) return initialised;
        var monitoring = await StartMonitoringAsync();
        if (monitoring.IsFailure) return monitoring;
        return Result.Ok();
    }

    public async Task Close() {
        if (_cancellationTokenSource is not null) {
            await _cancellationTokenSource?.CancelAsync()!;
        }
    }
    
    public async Task<IResult> InitializeAsync() {
        try {
            // Clear any previous existing states
            _previousTurnoutStates.Clear();
            _previousRouteStates.Clear();
            _previousOccupancyStates.Clear();
            _previousSignalStates.Clear();

            // Test connection to JMRI server
            var pingResult = await TestConnectionAsync();
            return !pingResult.IsSuccess ? pingResult : Result.Ok("Successfully initialized JMRI client");
        } catch (Exception ex) {
            return Result.Fail($"Failed to initialize JMRI client: {ex.Message}", ex);
        }
    }

    public async Task<IResult> TestConnectionAsync() {
        try {
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));
            var response = await HttpClient.GetAsync($"{_jmriUrl}/json", cancellationToken.Token);
            return response.IsSuccessStatusCode
                ? Result.Ok("JMRI server connection successful")
                : Result.Fail($"JMRI server returned status code: {response.StatusCode}");
        } catch (TaskCanceledException) {
            return Result.Fail($"Connection to JMRI server timed out after {ConnectionTimeoutSeconds} seconds");
        } catch (Exception ex) {
            return Result.Fail($"Failed to connect to JMRI server: {ex.Message}", ex);
        }
    }

    public async Task<IResult> ForceRefreshAsync(string? type = null) {
        switch (type?.ToLower() ?? "all") {
        case "turnout" or "turnouts": _previousTurnoutStates.Clear(); break;
        case "route" or "routes":     _previousRouteStates.Clear(); break;
        case "occupancy":             _previousOccupancyStates.Clear(); break;
        case "block" or "blocks":     _previousOccupancyStates.Clear(); break;
        case "signal" or "signals":   _previousSignalStates.Clear(); break;

        default:
            _previousTurnoutStates.Clear();
            _previousRouteStates.Clear();
            _previousOccupancyStates.Clear();
            _previousSignalStates.Clear();
            break;
        }
        await Task.CompletedTask;
        return Result.Ok();
    }

    public virtual async Task<IResult> StartMonitoringAsync() {
        try {
            _cancellationTokenSource = new CancellationTokenSource();
            var connectionResult = await ConnectWebSocketAsync();
            if (!connectionResult.IsSuccess) {
                return connectionResult;
            }

            var token = _cancellationTokenSource.Token;
            _ = Task.Run(() => ListenForEventsAsync(token), token);
            _ = Task.Run(() => MonitorWebSocketAsync(token), token);

            RaiseConnectionStatusChanged(true, "Monitoring started successfully");
            return Result.Ok("Monitoring started successfully");
        } catch (Exception ex) {
            RaiseConnectionStatusChanged(false, $"Failed to start monitoring: {ex.Message}");
            return Result.Fail($"Failed to start monitoring: {ex.Message}", ex);
        }
    }

    private async Task<IResult> ConnectWebSocketAsync() {
        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount < MaxConnectionRetries) {
            try {
                if (_webSocket.State == WebSocketState.Open) return Result.Ok("WebSocket already connected");

                _webSocket.Dispose();
                _webSocket = WebSocketFactory();
                var wsUri = $"{_jmriUrl}/json".Replace("http", "ws");
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));
                await _webSocket.ConnectAsync(new Uri(wsUri), timeoutCts.Token);

                // Expect a Hello message from the server
                // -------------------------------------------------------------------
                var serverHelloResult = await RecvResponseAsync(timeoutMs: 5000); 
                if (serverHelloResult.IsFailure) {
                    var errorMsg = $"Failed to receive server's hello message: {serverHelloResult.Message}";
                    Console.WriteLine(errorMsg); // Or use a proper logger
                    throw new Exception(errorMsg);
                } else {
                    Console.WriteLine($"Received HELLO from Server: {serverHelloResult.Value}");
                }
                
                // Send a Hello command to the server
                // -------------------------------------------------------------------
                var clientHelloCmd = BuildJmriMessage("hello", null, new Dictionary<string, object> { { "clientName", "DccPanelController" }, { "version", "1.0" } });
                var sendClientHelloResult = await SendCommandAsync(clientHelloCmd); // Just send
                if (sendClientHelloResult.IsFailure) {
                    var errorMsg = $"Failed to send client hello: {sendClientHelloResult?.Message}";
                    Console.WriteLine(errorMsg);
                    throw new Exception(errorMsg);
                } else {
                    Console.WriteLine($"Sent HELLO to Server: {sendClientHelloResult?.Message}");
                }
                
                RaiseConnectionStatusChanged(true, "WebSocket connected successfully");
                return Result.Ok("WebSocket connected successfully");
            } catch (Exception ex) {
                lastException = ex;
                retryCount++;

                if (retryCount < MaxConnectionRetries) {
                    await Task.Delay(ReconnectionDelayMs * retryCount); // Increasing delay for each retry
                }
            }
        }

        var errorMessage = $"WebSocket connection failed after {MaxConnectionRetries} attempts";
        RaiseConnectionStatusChanged(false, errorMessage);
        return Result.Fail(errorMessage, lastException ?? new Exception(errorMessage));
    }

    private async Task DisconnectWebSocketAsync() {
        try {
            if (_webSocket.State == WebSocketState.Open) {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Terminating Connection", CancellationToken.None);
            }
            _webSocket.Dispose();
            RaiseConnectionStatusChanged(false, "WebSocket disconnected");
        } catch (Exception ex) {
            Console.WriteLine($"WebSocket disconnect failed: {ex.Message}");
        }
    }

    private async Task MonitorWebSocketAsync(CancellationToken token) {
        var consecutiveFailures = 0;
        while (!token.IsCancellationRequested) {
            try {
                if (_webSocket.State != WebSocketState.Open) {
                    Console.WriteLine("WebSocket connection lost. Attempting to reconnect...");
                    var result = await ConnectWebSocketAsync();

                    if (result.IsSuccess) {
                        consecutiveFailures = 0;
                        Console.WriteLine("WebSocket reconnected successfully.");
                    } else {
                        consecutiveFailures++;
                        if (consecutiveFailures >= MaxConnectionRetries) {
                            var errorMessage = $"WebSocket reconnection failed after {MaxConnectionRetries} attempts. Stopping monitor.";
                            Console.WriteLine(errorMessage);
                            RaiseConnectionStatusChanged(false, errorMessage);
                            break;
                        }
                    }
                } else {
                    consecutiveFailures = 0;
                }

                await Task.Delay(ReconnectionDelayMs, token);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) {
                Console.WriteLine($"Error in WebSocket monitor: {ex.Message}");
                await Task.Delay(ReconnectionDelayMs, token);
            }
        }
        await DisconnectWebSocketAsync();
    }

    private async Task ListenForEventsAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            try {
                if (_webSocket.State == WebSocketState.Open) {
                    await PollForChanges("/json/turnouts", _previousTurnoutStates, ParseTurnoutData, TurnoutChanged, token);
                    await PollForChanges("/json/routes", _previousRouteStates, ParseRouteData, RouteChanged, token);
                    await PollForChanges("/json/blocks", _previousOccupancyStates, ParseOccupancyData, OccupancyChanged, token);
                    await PollForChanges("/json/signalMasts", _previousSignalStates, ParseSignalData, SignalChanged, token);
                }

                await Task.Delay(PollingIntervalMs, token);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) {
                Console.WriteLine($"Error in polling: {ex.Message}");
                await Task.Delay(ReconnectionDelayMs, token);
            }
        }
    }

    private async Task PollForChanges<T>(string endpoint,
                                         Dictionary<string, string> previousStates,
                                         Func<JsonElement, T?> parseData,
                                         EventHandler<T>? eventHandler,
                                         CancellationToken token,
                                         string? identifierField = "name",
                                         string? stateField = "state") where T : class {
        try {
            var currentData = await FetchInitialDataWithRetriesAsync(endpoint, 1);
            if (string.IsNullOrEmpty(currentData)) return;

            var items = JsonDocument.Parse(currentData).RootElement;
            foreach (var item in items.EnumerateArray()) {
                if (item.TryGetProperty("data", out var dataProperty) &&
                    dataProperty.TryGetProperty(identifierField ?? "name", out var nameProperty) &&
                    dataProperty.TryGetProperty(stateField ?? "state", out var stateProperty)) {
                    var name = nameProperty.ToString() ?? "";
                    var currentState = stateProperty.ToString();

                    // Check if this is a new item or its state has changed
                    if (!string.IsNullOrEmpty(name)) {
                        if (!previousStates.TryGetValue(name, out var previousState) || previousState != currentState) {
                            var args = parseData(item);
                            if (args != null && eventHandler != null) {
                                eventHandler.Invoke(this, args);
                            }
                            previousStates[name] = currentState;
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error polling {endpoint}: {ex.Message}");
        }
    }

    private async Task<string> FetchInitialDataWithRetriesAsync(string endpoint, int maxRetries = 3, int delayMilliseconds = 1000) {
        for (var attempt = 0; attempt < maxRetries; attempt++) {
            try {
                return await HttpClient.GetStringAsync($"{_jmriUrl}{endpoint}");
            } catch {
                if (attempt == maxRetries - 1) throw; // Rethrow on final attempt
            }
            await Task.Delay(delayMilliseconds * (int)Math.Pow(2, attempt)); // Exponential backoff
        }
        return string.Empty; // Fallback (should not be reached)
    }

    public virtual async Task<IResult> StopAsync() {
        try {
            if (_cancellationTokenSource is not null) {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            await DisconnectWebSocketAsync();
            return Result.Ok("Monitoring stopped successfully");
        } catch (Exception ex) {
            return Result.Fail($"Error stopping monitoring: {ex.Message}", ex);
        }
    }

    private async Task<string> FetchInitialDataAsync(string endpoint) {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"{_jmriUrl}{endpoint}");
        return response;
    }

    public virtual async Task<IResult> SendTurnoutCommandAsync(string identifier, bool thrown) {
        try {
            var command = BuildJmriMessage("turnout", "post", new Dictionary<string, object> {
                { "type", "turnout" },
                { "name", identifier },
                { "state", thrown ? 4 : 2 }
            });
            return await SendAndRecvAsync(command);
        } catch (Exception ex) {
            return Result.Fail($"Failed to send turnout command: {ex.Message}", ex);
        }
    }

    public virtual async Task<IResult> SendRouteCommandAsync(string identifier) {
        try {
            var command = BuildJmriMessage("route", "post", new Dictionary<string, object> {
                { "type", "route" },
                { "name", identifier },
                { "action", "set" }
            });
            return await SendCommandAsync(command);
        } catch (Exception ex) {
            return Result.Fail($"Failed to send route command: {ex.Message}", ex);
        }
    }

    public virtual async Task<IResult> SendSignalCommandAsync(string identifier, SignalAspectEnum aspect) {
        try {
            //{ "type" : "signalMast", "method" : "post", "data" : { "name": "Signal1",   "state": "Clear"   } }
            var command = BuildJmriMessage("signalMast", "post", new Dictionary<string, object> {
                { "name", identifier },
                { "set", aspect.ToString() }
            });
            return await SendCommandAsync(command);
        } catch (Exception ex) {
            return Result.Fail($"Failed to send signal command: {ex.Message}", ex);
        }
    }

    private async Task<IResult<string>> SendAndRecvAsync(string command) {
        var sendResponse = await SendCommandAsync(command);
        if (!sendResponse.IsSuccess) {
            Console.WriteLine($"Failed to send command: {sendResponse.Message}");
            return Result<string>.Fail("Failed to send message.");
        }

        var recvResponse = await RecvResponseAsync();
        if (!recvResponse.IsSuccess) {
            Console.WriteLine($"Failed to receive response: {recvResponse.Message}");
            return Result<string>.Fail("Failed to receive response.");
        }
        return Result<string>.Ok(recvResponse.Value);
    }

    private async Task<IResult<string>> RecvResponseAsync(int? timeoutMs = 100) {
        try {
            if (_webSocket.State != WebSocketState.Open) return Result<string>.Fail("WebSocket is not connected");
            var buffer = new byte[4096];
            var timeout = new CancellationTokenSource(timeoutMs ?? 100);
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), timeout.Token);
            var response = Encoding.UTF8.GetString(buffer, 0, result.Count);
            return Result<string>.Ok(response, "Response received successfully");
        } catch (Exception ex) {
            return Result<string>.Fail($"Failed to send command: {ex.Message}", ex);
        }
    }

    private async Task<IResult> SendCommandAsync(string command, int? timeoutMs = 100) {
        const int maxRetries = 3; // Original attempt + 1 retry
        for (int attempt = 0; attempt < maxRetries; attempt++) {
            try {
                if (_webSocket.State != WebSocketState.Open) {
                    var reconnectResult = await ConnectWebSocketAsync();
                    if (reconnectResult.IsFailure) {
                        if (attempt == maxRetries - 1) {
                            return Result.Fail($"WebSocket reconnection failed: {reconnectResult.Message}");
                        }
                        continue; 
                    }
                }
            
                var json = JsonSerializer.Serialize(command);
                var bytes = Encoding.ASCII.GetBytes(command);
                var timeout = new CancellationTokenSource(timeoutMs ?? 100);
                await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, timeout.Token);
                return Result.Ok("Command sent successfully");
            } 
            catch (WebSocketException ex) when (attempt < maxRetries - 1) {
                // WebSocket error occurred, try to reconnect on next iteration
                Console.WriteLine($"WebSocket error on attempt {attempt + 1}: {ex.Message}. Retrying...");
            }
            catch (OperationCanceledException ex) when (attempt < maxRetries - 1) {
                // Timeout occurred, try to reconnect on next iteration
                Console.WriteLine($"Timeout on attempt {attempt + 1}: {ex.Message}. Retrying...");
            }
            catch (Exception ex) {
                return Result.Fail($"Failed to send command: {ex.Message}", ex);
            }
        }
    
        return Result.Fail($"Failed to send command after {maxRetries} attempts");
    }
    
    private async Task<IResult> SendCommandAsyncNoRetry(string command, int? timeoutMs = 100) {
        try {
            if (_webSocket.State != WebSocketState.Open) return Result.Fail("WebSocket is not connected");
            var json = JsonSerializer.Serialize(command);
            var bytes = Encoding.ASCII.GetBytes(command);
            var timeout = new CancellationTokenSource(timeoutMs ?? 100);
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, timeout.Token);
            return Result.Ok("Command sent successfully");
        } catch (Exception ex) {
            return Result.Fail($"Failed to send command: {ex.Message}", ex);
        }
    }

    private TurnoutEventArgs? ParseTurnoutData(JsonElement data) {
        var dataStr = data.GetRawText();
        return !string.IsNullOrEmpty(dataStr) ? new TurnoutEventArgs(dataStr) : null;
    }

    private RouteEventArgs? ParseRouteData(JsonElement data) {
        var dataStr = data.GetRawText();
        return !string.IsNullOrEmpty(dataStr) ? new RouteEventArgs(dataStr) : null;
    }

    private OccupancyEventArgs? ParseOccupancyData(JsonElement data) {
        var dataStr = data.GetRawText();
        return !string.IsNullOrEmpty(dataStr) ? new OccupancyEventArgs(dataStr) : null;
    }

    private SignalEventArgs? ParseSignalData(JsonElement data) {
        var dataStr = data.GetRawText();
        return !string.IsNullOrEmpty(dataStr) ? new SignalEventArgs(dataStr) : null;
    }

    private void RaiseConnectionStatusChanged(bool isConnected, string message) {
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs(isConnected, message));
    }

    public static string BuildJmriMessage(string type, string? method, Dictionary<string, object>? parameters) {
        var message = new Dictionary<string, object?> { ["type"] = type };
        if (method is not null) message["method"] = method;
        if (parameters is { Count: > 0 }) {
            message["data"] = parameters;
        }
        return JsonSerializer.Serialize(message);
    }
}

public class ConnectionStatusEventArgs : System.EventArgs {
    public ConnectionStatusEventArgs(bool isConnected, string message) {
        IsConnected = isConnected;
        Message = message;
        Timestamp = DateTime.Now;
    }

    public bool IsConnected { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }
}