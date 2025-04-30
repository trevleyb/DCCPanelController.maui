using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DCCClients.Common;
using DCCClients.Jmri.JMRI.Commands;
using DCCClients.Jmri.JMRI.DataBlocks;
using DCCClients.Jmri.JMRI.EventArgs;

namespace DCCClients.Jmri.JMRI;

public class JmriClient {
    private static readonly HttpClient _httpClient = new() {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly string _jmriUrl;
    private readonly Dictionary<string, string> _previousTurnoutStates = new();
    private readonly Dictionary<string, string> _previousRouteStates = new();
    private readonly Dictionary<string, string> _previousOccupancyStates = new();
    private readonly Dictionary<string, string> _previousSignalStates = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private IWebSocket _webSocket = new WebSocketWrapper();

    public JmriClient(IDccSettings settings) {
        if (settings is JmriSettings info) {
            _jmriUrl = info.JmriServerUrl.TrimEnd('/');
        } else throw new ArgumentException("Invalid settings provided.");
    }

    public Func<HttpClient> HttpClientFactory { get; set; } = () => new HttpClient();
    public Func<IWebSocket> WebSocketFactory { get; set; } = () => new WebSocketWrapper();

    public event EventHandler<TurnoutEventArgs>? TurnoutChanged;
    public event EventHandler<RouteEventArgs>? RouteChanged;
    public event EventHandler<OccupancyEventArgs>? OccupancyChanged;
    public event EventHandler<SignalEventArgs>? SignalChanged;

    public virtual async Task InitializeAsync() {

        // Clear any previous existing states. These will be updated when we initially poll for 
        // the data and will update the dictionaries for tracking data changes. 
        // ------------------------------------------------------------------------------------
        _previousTurnoutStates.Clear();
        _previousRouteStates.Clear();
        _previousOccupancyStates.Clear();
        _previousSignalStates.Clear();
        
        Console.WriteLine("------------------------------------------------------------------------------------");
        Console.WriteLine("INITIAL DATA FETCH FINISHED ::: Waiting on Events...");
        Console.WriteLine("------------------------------------------------------------------------------------");
    }

    public virtual async Task StartMonitoringAsync() {
        _cancellationTokenSource = new CancellationTokenSource();
        await ConnectWebSocketAsync();
        var token = _cancellationTokenSource.Token;
        _ = Task.Run(() => ListenForEventsAsync(token), token);
        _ = Task.Run(() => MonitorWebSocketAsync(token), token);
    }

    private async Task ConnectWebSocketAsync() {
        try {
            if (_webSocket.State == WebSocketState.Open) return;
            _webSocket?.Dispose(); // Dispose of the previous instance if needed
            _webSocket = WebSocketFactory();
            var wsUri = $"{_jmriUrl}/json".Replace("http", "ws");
            await _webSocket.ConnectAsync(new Uri(wsUri), CancellationToken.None);
            Console.WriteLine("WebSocket connected.");
        } catch (Exception ex) {
            Console.WriteLine($"WebSocket connection failed: {ex.Message}");
            throw; // Rethrow to let the caller handle retries
        }
    }

    private async Task DisconnectWebSocketAsync() {
        try {
            if (_webSocket.State == WebSocketState.Open) {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Terminating Connection", CancellationToken.None);
            }
            _webSocket?.Dispose(); // Dispose of the previous instance if needed
            Console.WriteLine("WebSocket disconnected.");
        } catch (Exception ex) {
            Console.WriteLine($"WebSocket disconnect failed: {ex.Message}");
            throw; // Rethrow to let the caller handle retries
        }
    }
    
    private async Task MonitorWebSocketAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            Console.WriteLine($"MonitorWebSocketAsync running... State={_webSocket.State}");
            if (_webSocket.State != WebSocketState.Open) {
                Console.WriteLine("WebSocket connection lost. Attempting to reconnect...");
                try {
                    await ConnectWebSocketAsync();
                    Console.WriteLine("WebSocket reconnected.");
                } catch (Exception ex) {
                    Console.WriteLine($"Reconnection failed: {ex.Message}");
                }
            }
            await Task.Delay(5000, token); // Use token-aware delay
        }
        await DisconnectWebSocketAsync();
        Console.WriteLine("MonitorWebSocketAsync stopped.");
    }

    private async Task ListenForEventsAsync(CancellationToken token) {
        // Define polling interval
        const int pollingIntervalMs = 1000; // Poll every second, adjust as needed

        Console.WriteLine("Starting polling for state changes...");
        while (!token.IsCancellationRequested) {
            try {
                if (_webSocket.State == WebSocketState.Open) {
                    await PollForChanges("/json/turnouts", _previousTurnoutStates, ParseTurnoutData, TurnoutChanged, token);
                    await PollForChanges("/json/routes", _previousRouteStates, ParseRouteData, RouteChanged, token);
                    await PollForChanges("/json/blocks", _previousOccupancyStates, ParseOccupancyData, OccupancyChanged, token);
                    await PollForChanges("/json/signalMasts", _previousSignalStates, ParseSignalData, SignalChanged, token);
                }
                await Task.Delay(pollingIntervalMs, token);
            } catch (OperationCanceledException) {
                Console.WriteLine("Polling task canceled.");
                break;
            } catch (Exception ex) {
                Console.WriteLine($"Error in polling: {ex.Message}");
                await Task.Delay(2000, token);
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
                    var currentState = stateProperty.ToString()?? "";;

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
                //using var httpClient = _httpClient;
                Debug.WriteLine($"{_jmriUrl}{endpoint}");
                return await _httpClient.GetStringAsync($"{_jmriUrl}{endpoint}");
            } catch (Exception ex) {
                Console.WriteLine($"Attempt {attempt + 1} to fetch {endpoint} failed: {ex.Message}");
                if (attempt == maxRetries - 1) throw; // Rethrow on final attempt
            }

            await Task.Delay(delayMilliseconds * (int)Math.Pow(2, attempt)); // Exponential backoff
        }

        return string.Empty; // Fallback (should not be reached)
    }

    public virtual async Task StopAsync() {
        if (_cancellationTokenSource is not null) {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_webSocket.State == WebSocketState.Open) {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutting down", CancellationToken.None);
        }

        _webSocket.Dispose();
    }

    private async Task<string> FetchInitialDataAsync(string endpoint) {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"{_jmriUrl}{endpoint}");
        return response;
    }

    public virtual async Task SendTurnoutCommandAsync(string identifier, bool thrown) {
        var command = new { type = "turnout", identifier, state = thrown ? "THROWN" : "CLOSED" };
        await SendCommandAsync(command);
    }

    public virtual async Task SendRouteCommandAsync(string routeIdentifier) {
        var command = new { type = "route", identifier = routeIdentifier };
        await SendCommandAsync(command);
    }

    public virtual async Task SendSignalCommandAsync(string identifier, SignalAspectEnum aspect) {
        var signalCommand = new SignalCommand(identifier, aspect);
        var command = new { type = "signalHead", identifier, state = signalCommand.GetAspectString() };
        await SendCommandAsync(command);
    }

    private async Task SendCommandAsync(object command) {
        var json = JsonSerializer.Serialize(command);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
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
}