using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DCCClients.Common;
using DCCClients.Jmri.JMRI.Commands;
using DCCClients.Jmri.JMRI.EventArgs;

namespace DCCClients.Jmri.JMRI;

public class JmriClient {
    private static readonly HttpClient HttpClient = new() {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly string _jmriUrl;
    private readonly Dictionary<string, string> _previousTurnoutStates = new();
    private readonly Dictionary<string, string> _previousRouteStates = new();
    private readonly Dictionary<string, string> _previousOccupancyStates = new();
    private readonly Dictionary<string, string> _previousSignalStates = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private IWebSocket _webSocket = new WebSocketWrapper();

    // Configuration properties
    public int ConnectionTimeoutSeconds { get; set; } = 10;
    public int MaxConnectionRetries { get; set; } = 5;
    public int PollingIntervalMs { get; set; } = 5000;
    public int ReconnectionDelayMs { get; set; } = 2000;

    // Event to notify about connection status changes
    public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;

    public JmriClient(IDccSettings settings) {
        if (settings is JmriSettings info) {
            _jmriUrl = info.Url.TrimEnd('/');
        } else throw new ArgumentException("Invalid settings provided.");
    }

    public Func<HttpClient> HttpClientFactory { get; set; } = () => new HttpClient();
    public Func<IWebSocket> WebSocketFactory { get; set; } = () => new WebSocketWrapper();

    public event EventHandler<TurnoutEventArgs>? TurnoutChanged;
    public event EventHandler<RouteEventArgs>? RouteChanged;
    public event EventHandler<OccupancyEventArgs>? OccupancyChanged;
    public event EventHandler<SignalEventArgs>? SignalChanged;

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

    private async Task<IResult> TestConnectionAsync() {
        try {
            // Simple test to check if server is reachable
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));
            var response = await HttpClient.GetAsync($"{_jmriUrl}/json", cancellationToken.Token);

            return response.IsSuccessStatusCode 
                ? Result.Ok("JMRI server connection successful") 
                : Result.Fail($"JMRI server returned status code: {response.StatusCode }");
        } catch (TaskCanceledException) {
            return Result.Fail($"Connection to JMRI server timed out after {ConnectionTimeoutSeconds} seconds");
        } catch (Exception ex) {
            return Result.Fail($"Failed to connect to JMRI server: {ex.Message}", ex);
        }
    }

    public void ForceRefresh(string? type) {
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

                Console.WriteLine("WebSocket connected successfully.");
                RaiseConnectionStatusChanged(true, "WebSocket connected successfully");
                return Result.Ok("WebSocket connected successfully");
            } catch (Exception ex) {
                lastException = ex;
                retryCount++;

                if (retryCount < MaxConnectionRetries) {
                    Console.WriteLine($"WebSocket connection attempt {retryCount} failed: {ex.Message}. Retrying...");
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
            Console.WriteLine("WebSocket disconnected.");
            RaiseConnectionStatusChanged(false, "WebSocket disconnected");
        } catch (Exception ex) {
            // Don't throw - just log the error
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
        Console.WriteLine("MonitorWebSocketAsync stopped.");
    }

    private async Task ListenForEventsAsync(CancellationToken token) {
        Console.WriteLine("Starting polling for state changes...");

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
                Console.WriteLine("Polling task canceled.");
                break;
            } catch (Exception ex) {
                Console.WriteLine($"Error in polling: {ex.Message}");
                await Task.Delay(ReconnectionDelayMs, token);
            }
        }

        Console.WriteLine("ListenForEventsAsync stopped.");
    }

    private async Task PollForChanges<T>(string endpoint,
                                         Dictionary<string, string> previousStates,
                                         Func<JsonElement, T?> parseData,
                                         EventHandler<T>? eventHandler,
                                         CancellationToken token,
                                         string? identifierField = "name",
                                         string? stateField = "state") where T : class {
        try {
            var currentData = await FetchInitialDataWithRetriesAsync(endpoint, maxRetries: 1);
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
                Debug.WriteLine($"{_jmriUrl}{endpoint}");
                return await HttpClient.GetStringAsync($"{_jmriUrl}{endpoint}");
            } catch (Exception ex) {
                Console.WriteLine($"Attempt {attempt + 1} to fetch {endpoint} failed: {ex.Message}");
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
            var command = new { type = "turnout", identifier, state = thrown ? "THROWN" : "CLOSED" };
            return await SendCommandAsync(command);
        } catch (Exception ex) {
            return Result.Fail($"Failed to send turnout command: {ex.Message}", ex);
        }
    }

    public virtual async Task<IResult> SendRouteCommandAsync(string routeIdentifier) {
        try {
            var command = new { type = "route", identifier = routeIdentifier };
            return await SendCommandAsync(command);
        } catch (Exception ex) {
            return Result.Fail($"Failed to send route command: {ex.Message}", ex);
        }
    }

    public virtual async Task<IResult> SendSignalCommandAsync(string identifier, SignalAspectEnum aspect) {
        try {
            var signalCommand = new SignalCommand(identifier, aspect);
            var command = new { type = "signalHead", identifier, state = signalCommand.GetAspectString() };
            return await SendCommandAsync(command);
        } catch (Exception ex) {
            return Result.Fail($"Failed to send signal command: {ex.Message}", ex);
        }
    }

    private async Task<IResult> SendCommandAsync(object command) {
        try {
            if (_webSocket.State != WebSocketState.Open) {
                return Result.Fail("WebSocket is not connected");
            }

            var json = JsonSerializer.Serialize(command);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
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
}

public class ConnectionStatusEventArgs : System.EventArgs {
    public bool IsConnected { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }

    public ConnectionStatusEventArgs(bool isConnected, string message) {
        IsConnected = isConnected;
        Message = message;
        Timestamp = DateTime.Now;
    }
}