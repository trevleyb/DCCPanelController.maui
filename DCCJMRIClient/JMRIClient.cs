using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DCCJmriClient.EventArgs;

namespace DCCJmriClient;

public class JmriClient {
    private static readonly HttpClient _httpClient = new() {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly string _jmriUrl;

    private CancellationTokenSource? _cancellationTokenSource;
    private IWebSocket _webSocket = new WebSocketWrapper();

    public JmriClient(string jmriUrl) {
        _jmriUrl = jmriUrl.TrimEnd('/');
        TurnoutChanged += (_, args) => DumpObjectProperties(args);
        RouteChanged += (_, args) => DumpObjectProperties(args);
        OccupancyChanged += (_, args) => DumpObjectProperties(args);
    }

    public Func<HttpClient> HttpClientFactory { get; set; } = () => new HttpClient();
    public Func<IWebSocket> WebSocketFactory { get; set; } = () => new WebSocketWrapper();

    public event EventHandler<TurnoutEventArgs>? TurnoutChanged;
    public event EventHandler<RouteEventArgs>? RouteChanged;
    public event EventHandler<OccupancyEventArgs>? OccupancyChanged;

    public async Task InitializeAsync() {
        // Fetch initial turnout data and raise events
        var turnoutData = await FetchInitialDataWithRetriesAsync("/json/turnouts");
        if (!string.IsNullOrEmpty(turnoutData)) {
            var turnouts = JsonDocument.Parse(turnoutData).RootElement;
            foreach (var turnout in turnouts.EnumerateArray()) {
                var args = ParseTurnoutData(turnout);
                TurnoutChanged?.Invoke(this, args);
            }
        }

        // Fetch initial route data and raise events
        var routeData = await FetchInitialDataWithRetriesAsync("/json/routes");
        if (!string.IsNullOrEmpty(routeData)) {
            var routes = JsonDocument.Parse(routeData).RootElement;
            foreach (var route in routes.EnumerateArray()) {
                var args = ParseRouteData(route);
                RouteChanged?.Invoke(this, args);
            }
        }

        // Fetch initial occupancy data and raise events
        var occupancyData = await FetchInitialDataWithRetriesAsync("/json/blocks");
        if (!string.IsNullOrEmpty(occupancyData)) {
            var blocks = JsonDocument.Parse(occupancyData).RootElement;
            foreach (var block in blocks.EnumerateArray()) {
                var args = ParseOccupancyData(block);
                OccupancyChanged?.Invoke(this, args);
            }
        }
    }

    public async Task StartMonitoringAsync() {
        _cancellationTokenSource = new CancellationTokenSource();
        await ConnectWebSocketAsync();
        var token = _cancellationTokenSource.Token;
        _ = Task.Run(() => ListenForEventsAsync(token), token);
        _ = Task.Run(() => MonitorWebSocketAsync(token), token);
    }

    private async Task ConnectWebSocketAsync() {
        try {
            if (_webSocket.State == WebSocketState.Open) return;

            _webSocket.Dispose(); // Dispose of the previous instance if needed
            _webSocket = WebSocketFactory();

            await _webSocket.ConnectAsync(new Uri($"{_jmriUrl}/websocket"), CancellationToken.None);
            Console.WriteLine("WebSocket connected.");
        } catch (Exception ex) {
            Console.WriteLine($"WebSocket connection failed: {ex.Message}");
            throw; // Rethrow to let the caller handle retries
        }
    }

    private async Task MonitorWebSocketAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
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

        Console.WriteLine("MonitorWebSocketAsync stopped.");
    }

    private async Task ListenForEventsAsync(CancellationToken token) {
        var buffer = new byte[4096];
        while (!token.IsCancellationRequested && _webSocket.State == WebSocketState.Open) {
            try {
                var result = await _webSocket.ReceiveAsync(buffer, token);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Parse the JSON message and raise appropriate events
                var data = JsonDocument.Parse(message);
                var root = data.RootElement;

                if (root.TryGetProperty("type", out var type)) {
                    switch (type.GetString()) {
                    case "turnout":
                        var turnoutArgs = ParseTurnoutData(root);
                        TurnoutChanged?.Invoke(this, turnoutArgs);
                        break;

                    case "route":
                        var routeArgs = ParseRouteData(root);
                        RouteChanged?.Invoke(this, routeArgs);
                        break;

                    case "block":
                        var occupancyArgs = ParseOccupancyData(root);
                        OccupancyChanged?.Invoke(this, occupancyArgs);
                        break;
                    }
                }
            } catch (OperationCanceledException) {
                // Gracefully handle cancellation
                Console.WriteLine("Listener task canceled.");
                break;
            } catch (Exception ex) {
                Console.WriteLine($"Error in WebSocket listener: {ex.Message}");
            }
        }

        Console.WriteLine("ListenForEventsAsync stopped.");
    }

    private async Task<string> FetchInitialDataWithRetriesAsync(string endpoint, int maxRetries = 3, int delayMilliseconds = 1000) {
        for (var attempt = 0; attempt < maxRetries; attempt++) {
            try {
                using var httpClient = _httpClient;
                return await httpClient.GetStringAsync($"{_jmriUrl}{endpoint}");
            } catch (Exception ex) {
                Console.WriteLine($"Attempt {attempt + 1} to fetch {endpoint} failed: {ex.Message}");
                if (attempt == maxRetries - 1) throw; // Rethrow on final attempt
            }

            await Task.Delay(delayMilliseconds * (int)Math.Pow(2, attempt)); // Exponential backoff
        }

        return string.Empty; // Fallback (should not be reached)
    }

    public async Task StopAsync() {
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

    public async Task SendTurnoutCommandAsync(string identifier, bool set) {
        var command = new { type = "turnout", identifier, state = set ? "THROWN" : "CLOSED" };
        await SendCommandAsync(command);
    }

    public async Task SendRouteCommandAsync(string routeIdentifier) {
        var command = new { type = "route", identifier = routeIdentifier };
        await SendCommandAsync(command);
    }

    private async Task SendCommandAsync(object command) {
        var json = JsonSerializer.Serialize(command);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private TurnoutEventArgs ParseTurnoutData(JsonElement data) {
        return new TurnoutEventArgs {
            Identifier = GetJsonValue<string>(data, "name") ?? "UNKNOWN",
            DccAddress = GetJsonValue<int>(data, "dccAddress"),
            State = GetJsonValue<string>(data, "state") ?? "UNKNOWN",
            IsLocked = GetJsonValue<bool>(data, "locked")
        };
    }

    private RouteEventArgs ParseRouteData(JsonElement data) {
        var routeStates = new Dictionary<string, string>();

        if (data.TryGetProperty("turnouts", out var turnouts) && turnouts.ValueKind == JsonValueKind.Array) {
            foreach (var turnout in turnouts.EnumerateArray()) {
                var turnoutId = GetJsonValue<string>(data, "name") ?? "UNKNOWN";
                var turnoutState = GetJsonValue<string>(data, "state") ?? "UNKNOWN";
                routeStates[turnoutId] = turnoutState;
            }
        }

        return new RouteEventArgs {
            Identifier = GetJsonValue<string>(data, "name") ?? "UNKNOWN",
            State = GetJsonValue<string>(data, "state") ?? "UNKNOWN",
            TurnoutStates = routeStates,
            Metadata = GetJsonValue<string>(data, "metadata")
        };
    }

    private OccupancyEventArgs ParseOccupancyData(JsonElement data) {
        return new OccupancyEventArgs {
            Identifier = GetJsonValue<string>(data, "name") ?? "UNKNOWN",
            IsOccupied = GetJsonValue<bool>(data, "occupied"),
            TrainId = GetJsonValue<string>(data, "trainId") ?? "UNKNOWN",
            Metadata = GetJsonValue<string>(data, "metadata")
        };
    }

    private T? GetJsonValue<T>(JsonElement element, string propertyName) {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null) {
            return JsonSerializer.Deserialize<T>(property.GetRawText());
        }

        return default;
    }

    public static void DumpObjectProperties(object? obj) {
        if (obj == null) {
            Console.WriteLine("Object is null.");
            return;
        }

        var type = obj.GetType();
        Console.WriteLine($"Dumping properties for {type.Name}:");

        foreach (var property in type.GetProperties()) {
            try {
                var value = property.GetValue(obj) ?? "null";
                Console.WriteLine($"  {property.Name}: {value}");
            } catch (Exception ex) {
                Console.WriteLine($"  {property.Name}: Error retrieving value ({ex.Message})");
            }
        }
    }
}