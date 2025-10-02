using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using DCCPanelController.Clients.Discovery;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Clients.Jmri.Events;
using DCCPanelController.Helpers; // JMRI event args

namespace DCCPanelController.Clients.Jmri;

/// <summary>
/// Single-class JMRI client:
/// - : DccClientBase, IDccClient
/// - Owns its WebSocket, heartbeat, refresh, subscribe
/// - Parses inbound JSON with your Jmri*EventArgs and updates Profile directly
/// - No proxy, no inner client
/// </summary>
public sealed class JmriDccClient : DccClientBase, IDccClient, IDisposable {
    private readonly JmriSettings _jmriSettings;

    // -------------------- Transport --------------------
    private ClientWebSocket?            _ws;
    private CancellationTokenSource?    _cts;
    private Task?                       _connectionTask;
    private TaskCompletionSource<bool>? _handshakeTcs;

    private          Timer? _heartbeat; // JMRI heartbeat (ping interval comes from server hello)
    private          Timer? _refresh;   // periodic "list" to keep updates flowing
    private readonly object _sync = new();
    private volatile bool   _disposed;

    private const int MinRefreshMs            = 250;
    private const int DefaultReconnectDelayMs = 5000;

    // -------------------- Caches for change detection --------------------
    private readonly ConcurrentDictionary<string, JmriTurnoutEventArgs> _turnouts = new();
    private readonly ConcurrentDictionary<string, JmriSignalEventArgs>  _signals  = new();
    private readonly ConcurrentDictionary<string, JmriRouteEventArgs>   _routes   = new();
    private readonly ConcurrentDictionary<string, JmriSensorEventArgs>  _sensors  = new();
    private readonly ConcurrentDictionary<string, JmriBlockEventArgs>   _blocks   = new();
    private readonly ConcurrentDictionary<string, JmriLightEventArgs>   _lights   = new();

    // -------------------- Public shape --------------------
    public JmriDccClient(Profile profile, IDccClientSettings settings) : base(profile) {
        if (settings is JmriSettings jmriSettings) {
            _jmriSettings = jmriSettings;
        } else throw new ArgumentNullException(nameof(settings));
    }

    public DccClientType Type => DccClientType.Jmri;

    public async Task<IResult> UpdateAutomaticSettings() {
        // If auto, try to discover and update the bound settings object
        if (_jmriSettings.SetAutomatically) {
            var auto = await GetAutomaticConnectionDetailsAsync();
            if (auto.IsFailure || auto.Value is null || auto.Value is not JmriSettings) {
                State = DccClientState.Disconnected;
                OnClientMessage("Could not automatically set connection details. Is JMRI Server running?");
                return Result.Fail(auto.Message ?? "Autodetect failed.");
            }
            _jmriSettings.Address = ((JmriSettings)auto.Value).Address;
            _jmriSettings.Port = ((JmriSettings)auto.Value).Port;
            OnClientMessage($"Found server at {_jmriSettings.Address}:{_jmriSettings.Port}");
        }
        return Result.Ok();
    }
    
    // -------------------- Lifecycle --------------------
    public async Task<IResult> ConnectAsync() {
        // 1) Try autodetect (keeps your current behavior)
        // var settings = await UpdateAutomaticSettings();
        // if (settings.IsFailure) {
        //     State = DccClientState.Disconnected;
        //     OnClientMessage("Could not auto-detect connection settings.");
        //     return Result.Fail("Could not auto-detect connection settings.");
        // }

        // 2) Hard probe: fail-fast if we can't connect/handshake now
        var probe = await ValidateConnectionAsync();
        if (probe.IsFailure) {
            State = DccClientState.Error;
            OnClientMessage($"Initial connect failed: {probe.Message}", DccClientOperation.System, DccClientMessageType.Error);
            return Result.Fail(probe.Message ?? "Initial connect failed");
        }

        // 3) Only now start the reconnect loop for post-disconnect recovery
        lock (_sync) {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _connectionTask = RunReconnectLoopAsync(
                connectOnce: ConnectAndListenAsync,
                isDisposed: () => _disposed,
                maxRetries: _jmriSettings.MaxRetries <= 0 ? int.MaxValue : _jmriSettings.MaxRetries,
                initialDelay: TimeSpan.FromMilliseconds(_jmriSettings.InitialBackoffMs <= 0 ? 1000 : _jmriSettings.InitialBackoffMs),
                multiplier: _jmriSettings.BackoffMultiplier <= 1 ? 1 : _jmriSettings.BackoffMultiplier,
                ct: _cts.Token
            );
        }

        State = DccClientState.Initialising;
        OnClientMessage($"Connecting to JMRI at {_jmriSettings.Address}:{_jmriSettings.Port}...");
        return Result.Ok();
    }

    public async Task<IResult> DisconnectAsync() {
        StopHeartbeat();
        StopRefreshTimer();

        // Send a goodbye (best-effort)
        try {
            await SendCommandAsync(JmriHandshakeEventArgs.GoodbyeMessage);
        } catch { /* ignore */
        }

        CancellationTokenSource? local;
        Task? t;
        lock (_sync) {
            local = _cts;
            _cts = null;
            t = _connectionTask;
            _connectionTask = null;
        }

        try {
            if (local is { }) {
                await local.CancelAsync();
                try {
                    if (t is { }) await t.WaitAsync(TimeSpan.FromSeconds(5), local.Token);
                } catch { /* ignore */
                }
            }
        } catch { /* ignore */
        }

        CloseSocket();

        State = DccClientState.Disconnected;
        OnClientMessage("Disconnected from JMRI");
        return Result.Ok();
    }

    public async Task<IResult> ValidateConnectionAsync() {
        try {
            var result = await UpdateAutomaticSettings();
            if (result.IsFailure) {
                OnClientMessage("Could not auto-detect connection settings.");
                return Result.Fail("Could not auto-detect connection settings.");
            }
            
            using var ws = new ClientWebSocket();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var uri = new Uri($"ws://{_jmriSettings.Address}:{_jmriSettings.Port}/json/");
            await ws.ConnectAsync(uri, cts.Token);

            // wait for a 'hello' ON THIS TEMP SOCKET
            var buf = new byte[4096];
            var sb = new StringBuilder();
            while (!cts.IsCancellationRequested && ws.State == WebSocketState.Open) {
                var res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), cts.Token);
                if (res.MessageType == WebSocketMessageType.Close) break;
                sb.Append(Encoding.UTF8.GetString(buf, 0, res.Count));
                if (res.EndOfMessage && sb.Length > 0) {
                    using var doc = JsonDocument.Parse(sb.ToString());
                    var root = doc.RootElement.ValueKind == JsonValueKind.Array
                        ? doc.RootElement.EnumerateArray().FirstOrDefault()
                        : doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object &&
                        root.TryGetProperty("type", out var t) && t.GetString() == "hello") {
                        return Result.Ok("JMRI handshake OK");
                    }
                    sb.Clear();
                }
            }
            return Result.Fail("JMRI handshake not received");
        } catch (Exception ex) {
            return Result.Fail(ex, "JMRI probe failed");
        }
    }

    public async Task<IResult> SetAutomaticSettingsAsync() {
        var auto = await GetAutomaticConnectionDetailsAsync();
        if (auto.IsFailure || auto.Value is null || auto.Value is not JmriSettings settings) return Result.Fail(auto.Message ?? "No JMRI servers found.");
        _jmriSettings.Address = settings.Address;
        _jmriSettings.Port = settings.Port;
        return Result.Ok();
    }

    public async Task<IResult> ForceRefreshAsync(DccClientCapability? _ = null) {
        ResetCaches();
        await SubscribeToUpdatesAsync(); // nudge a relist now
        OnClientMessage("JMRI: forced relist");
        return Result.Ok();
    }

    // -------------------- Discovery --------------------
    public async Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() {
        try {
            var result = await DiscoverServices.SearchForJmriServicesAsync();
            if (result is { IsSuccess: true, Value.Count: > 0 }) return result;
            return Result<List<DiscoveredService>>.Fail("Unable to find a JMRI server.");
        } catch (Exception ex) {
            return Result<List<DiscoveredService>>.Fail(ex, "Unable to find a JMRI server.");
        }
    }

    public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
        var found = await FindAvailableServicesAsync();
        if (found.IsFailure || found.Value is null || found.Value.Count == 0)
            return Result<IDccClientSettings?>.Fail("No available servers could be found.");

        foreach (var server in found.Value) {
            if (server.Address.ToString() != "0.0.0.0" &&  server.Address.ToString() != "127.0.0.1") {
                var rawSettings = new JmriSettings {
                    Address = server.Address.ToString(),
                    Port = server.Port,
                };
                return Result<IDccClientSettings?>.Ok(rawSettings);
            }
        }
        return Result<IDccClientSettings?>.Fail("No available servers could be found.");
    }

    // -------------------- Commands --------------------
    public Task<IResult> SendTurnoutCmdAsync(Turnout t, bool thrown) => SendWrap(() => SetTurnoutStateAsync(t.Id ?? t.Name ?? "", thrown),
        $"Setting turnout {t.Name}({t.Id}) to {(thrown ? "THROWN" : "CLOSED")}",
        DccClientOperation.Turnout);

    public Task<IResult> SendRouteCmdAsync(Route r, bool active) => SendWrap(() => SetRouteStateAsync(r.Id ?? r.Name ?? "", active),
        $"Setting route {r.Name}({r.Id}) to {(active ? "ACTIVE" : "INACTIVE")}",
        DccClientOperation.Route);

    public Task<IResult> SendSignalCmdAsync(Signal s, SignalAspectEnum aspect) => SendWrap(() => SetSignalAppearanceAsync(s.Id ?? s.Name ?? "", aspect.ToString()),
        $"Setting signal {s.Name}({s.Id}) to {aspect}",
        DccClientOperation.Signal);

    public Task<IResult> SendLightCmdAsync(Light l, bool on) => SendWrap(() => SetLightStateAsync(l.Id ?? l.Name ?? "", on),
        $"Setting light {l.Name}({l.Id}) to {(on ? "ON" : "OFF")}",
        DccClientOperation.Light);

    public Task<IResult> SendBlockCmdAsync(Block b, bool allocated) => SendWrap(() => SetBlockAllocatedAsync(b.Id ?? b.Name ?? "", allocated),
        $"Setting block {b.Name}({b.Id}) to {(allocated ? "OCCUPIED" : "FREE")}",
        DccClientOperation.Block);

    public Task<IResult> SendSensorCmdAsync(Sensor s, bool active) => SendWrap(() => SetSensorStateAsync(s.Id ?? s.Name ?? "", active),
        $"Setting sensor {s.Name}({s.Id}) to {(active ? "ON" : "OFF")}",
        DccClientOperation.Sensor);

    private async Task<IResult> SendWrap(Func<Task> send, string log, DccClientOperation op) {
        if (_ws is not { State: WebSocketState.Open }) return Result.Fail("Not connected to JMRI server");
        try {
            await send();
            OnClientMessage(log, op, DccClientMessageType.Outbound);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(ex, $"Failed to send {op} command to JMRI server");
        }
    }

    // -------------------- Transport & Protocol --------------------
    private async Task MaintainConnectionAsync(CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            try {
                await ConnectAndListenAsync(ct);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) when (!ct.IsCancellationRequested) {
                State = DccClientState.Disconnected;
                OnClientMessage($"JMRI connection error: {ex.Message}", DccClientOperation.System, DccClientMessageType.Error);

                try {
                    await Task.Delay(DefaultReconnectDelayMs, ct);
                } catch (OperationCanceledException) {
                    break;
                }
            }
        }
    }

    private async Task ConnectAndListenAsync(CancellationToken ct) {
        State = DccClientState.Initialising;
        CloseSocket();

        var result = await UpdateAutomaticSettings();
        if (result.IsFailure) {
            State = DccClientState.Error;
            OnClientMessage("Could not auto-detect connection settings.");
            return;
        }

        _ws = new ClientWebSocket();
        _handshakeTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var uri = new Uri($"ws://{_jmriSettings.Address}:{_jmriSettings.Port}/json/");
        await _ws.ConnectAsync(uri, ct);

        State = DccClientState.Connected;
        OnClientMessage("Connected to JMRI server, waiting for handshake");

        // Start receive loop
        var listenTask = ListenForMessagesAsync(ct);

        // Await handshake or cancellation
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var completed = await Task.WhenAny(_handshakeTcs.Task, Task.Delay(Timeout.InfiniteTimeSpan, linked.Token));
        if (completed != _handshakeTcs.Task)
            ct.ThrowIfCancellationRequested();

        OnClientMessage("Handshake complete, subscribing to updates");
        StartRefreshTimer(int.Max((int)_jmriSettings.PollingInterval, MinRefreshMs));

        // Wait for the listen loop to exit (normal on close)
        await listenTask;

        // When listen exits, mark disconnected (reconnect loop will schedule the retry)
        StopHeartbeat();
        StopRefreshTimer();
        State = DccClientState.Disconnected;
        OnClientMessage("JMRI connection closed");
    }

    private async Task ListenForMessagesAsync(CancellationToken ct) {
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        while (_ws is { State: WebSocketState.Open } && !ct.IsCancellationRequested) {
            var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            if (result.MessageType == WebSocketMessageType.Text) {
                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                if (result.EndOfMessage) {
                    try {
                        ProcessMessage(sb.ToString());
                    } catch (Exception ex) {
                        OnClientMessage($"JMRI parse error: {ex.Message}", DccClientOperation.System, DccClientMessageType.Error);
                    } finally {
                        sb.Clear();
                    }
                }
            } else if (result.MessageType == WebSocketMessageType.Close) {
                break; // normal termination; outer caller changes state
            }
        }
    }

    private void ProcessMessage(string json) {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Items may arrive as a single object or array of objects
        if (root.ValueKind == JsonValueKind.Array) {
            foreach (var item in root.EnumerateArray())
                ProcessOne(item);
        } else {
            ProcessOne(root);
        }
    }

    private void ProcessOne(JsonElement item) {
        if (!item.TryGetProperty("type", out var typeEl)) return;
        var type = typeEl.GetString();
        if (string.IsNullOrEmpty(type)) return;

        var handled = type switch {
            "turnout"    => JmriTurnoutEventArgs.ProcessMessage(item, _turnouts, (_, e) => OnTurnoutChanged(e)),
            "signalHead" => JmriSignalEventArgs.ProcessMessage(item, _signals, (_, e) => OnSignalChanged(e)),
            "route"      => JmriRouteEventArgs.ProcessMessage(item, _routes, (_, e) => OnRouteChanged(e)),
            "sensor"     => JmriSensorEventArgs.ProcessMessage(item, _sensors, (_, e) => OnSensorChanged(e)),
            "block"      => JmriBlockEventArgs.ProcessMessage(item, _blocks, (_, e) => OnBlockChanged(e)),
            "light"      => JmriLightEventArgs.ProcessMessage(item, _lights, (_, e) => OnLightChanged(e)),
            "hello"      => ProcessHello(item),
            "goodbye"    => ProcessGoodbye(item),
            "error"      => ProcessError(item),
            "power"      => ProcessPower(item),
            "time"       => ProcessTime(item),
            "pong"       => true, // ignore
            _            => false
        };

        if (!handled) {
            Debug.WriteLine($"Unknown/invalid JMRI JSON: {type} => {item}");
        }
    }

    private bool ProcessHello(JsonElement root) {
        if (JmriHandshakeEventArgs.Create(root) is not { } hello) return false;

        // Server-supplied heartbeat interval (seconds)
        StartHeartbeat(hello.Heartbeat);

        _handshakeTcs?.TrySetResult(true);
        OnConnectionStateChanged("Connection to JMRI has been established.");
        return true;
    }

    private bool ProcessGoodbye(JsonElement _) {
        OnClientMessage("JMRI sent goodbye; closing without retry.", DccClientOperation.System, DccClientMessageType.System);

        // Stop periodic work
        StopHeartbeat();
        StopRefreshTimer();

        // Tell the outer RunReconnectLoopAsync to stop (no retry)
        _cts?.Cancel();

        // Close the transport so ListenForMessagesAsync returns
        CloseSocket();

        // Reflect final state
        State = DccClientState.Disconnected;
        return true;
    }

    private bool ProcessError(JsonElement root) {
        if (!root.TryGetProperty("data", out var data)) return false;
        var code = data.GetIntProperty("code");
        var msg = data.GetStringProperty("message");
        OnConnectionStateChanged($"JMRI error: {code} => {msg}");
        return true;
    }

    private bool ProcessPower(JsonElement root) {
        if (!root.TryGetProperty("data", out var data)) return false;
        var name = data.GetStringProperty("name");
        var state = data.GetIntProperty("state");
        var defaultState = data.GetBoolProperty("default");

        var stateEnum = state switch {
            0 => PowerStateEnum.Off,
            1 => PowerStateEnum.On,
            _ => PowerStateEnum.Unknown
        };

        if (Profile?.PowerState != stateEnum) {
            UpdatePowerState(stateEnum);
            OnClientMessage($"Power State : {(stateEnum)}", DccClientOperation.System, DccClientMessageType.Inbound);
        }
        return true;
    }
    
    private bool ProcessTime(JsonElement root) {
        if (!root.TryGetProperty("data", out var data)) return false;
        var time = data.GetTimeProperty("time");
        var rate = data.GetDoubleProperty("rate");
        var state = data.GetIntProperty("state");

        var stateEnum = state switch {
            0 => FastClockStateEnum.Off,
            1 => FastClockStateEnum.On,
            _ => FastClockStateEnum.Unknown
        };
        
        if (time is {}  && Profile?.FastClock != time) {
            UpdateFastClock(time, stateEnum);
            OnClientMessage($"Fast Clock Updated : {time:HH:mm:ss tt} @ {rate} State={state}", DccClientOperation.System, DccClientMessageType.Inbound);
        }
        return true;
    }

    // -------------------- Inbound → Profile updates --------------------
    private void OnTurnoutChanged(JmriTurnoutEventArgs e) {
        // JMRI: 2=Closed, 4=Thrown
        var closed = e.State == 2;
        UpdateTurnout(e.Name, e.UserName, closed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown);
        OnClientMessage($"Turnout {e.Name} {(closed ? "CLOSED" : "THROWN")}", DccClientOperation.Turnout, DccClientMessageType.Inbound);
    }

    private void OnRouteChanged(JmriRouteEventArgs e) {
        // JMRI: 2=Active, 4=Inactive
        var active = e.State == 2;
        UpdateRoute(e.Name, e.UserName, active ? RouteStateEnum.Active : RouteStateEnum.Inactive);
        OnClientMessage($"Route {e.Name} {(active ? "ACTIVE" : "INACTIVE")}", DccClientOperation.Route, DccClientMessageType.Inbound);
    }

    private void OnSensorChanged(JmriSensorEventArgs e) {
        // JMRI: 2=Active, 4=Inactive
        var on = e.State == 2;
        UpdateSensor(e.Name, e.UserName, on);
        OnClientMessage($"Sensor {e.Name} {(on ? "ON" : "OFF")}", DccClientOperation.Sensor, DccClientMessageType.Inbound);
    }

    private void OnLightChanged(JmriLightEventArgs e) {
        // JMRI: 2=Active, 4=Inactive
        var on = e.State == 2;
        UpdateLight(e.Name, e.UserName, on);
        OnClientMessage($"Light {e.Name} {(on ? "ON" : "OFF")}", DccClientOperation.Light, DccClientMessageType.Inbound);
    }

    private void OnBlockChanged(JmriBlockEventArgs e) {
        // JMRI: 2=Occupied, 4=Unoccupied
        var occ = e.State == 2;
        UpdateBlock(e.Name, e.UserName, occ, e.SensorName);
        OnClientMessage($"Block {e.Name} {(occ ? "OCCUPIED" : "FREE")}", DccClientOperation.Block, DccClientMessageType.Inbound);
    }

    private void OnSignalChanged(JmriSignalEventArgs e) {
        UpdateSignal(e.Name, e.UserName, SignalAspectEnum.Off);
        OnClientMessage($"Signal {e.Name} appearance={e.Appearance}", DccClientOperation.Signal, DccClientMessageType.Inbound);
    }

    // -------------------- Heartbeat & Refresh --------------------
    private void StartHeartbeat(int heartbeatSeconds) {
        StopHeartbeat();
        if (heartbeatSeconds <= 0) return;
        var intervalMs = Math.Max(1, heartbeatSeconds) * 1000;
        _heartbeat = new Timer(SendHeartbeat, null, intervalMs, intervalMs);
    }

    private void StopHeartbeat() {
        _heartbeat?.Dispose();
        _heartbeat = null;
    }

    private async void SendHeartbeat(object? _) {
        try {
            await SendMessageAsync(JsonSerializer.Serialize(new { type = "ping" }));
        } catch (Exception ex) {
            Debug.WriteLine($"JMRI heartbeat send failed: {ex.Message}");
        }
    }

    private void StartRefreshTimer(int refreshMs) {
        StopRefreshTimer();
        if (refreshMs <= 0) return;
        _refresh = new Timer(async void (_) => {
            try {
                if (State == DccClientState.Connected) await SubscribeToUpdatesAsync();
            } catch (Exception ex) {
                Debug.WriteLine($"JMRI refresh failed: {ex.Message}");
            }
        }, null, refreshMs, refreshMs);
    }

    private void StopRefreshTimer() {
        _refresh?.Dispose();
        _refresh = null;
    }

    private async Task SubscribeToUpdatesAsync() {
        var subs = new[] {
            new { type = "turnout", method = "list" },
            new { type = "signalHead", method = "list" },
            new { type = "route", method = "list" },
            new { type = "sensor", method = "list" },
            new { type = "light", method = "list" },
            new { type = "block", method = "list" },
        };
        foreach (var s in subs) await SendCommandAsync(JsonSerializer.Serialize(s));

        await SendCommandAsync(JsonSerializer.Serialize( new { type = "power" } ));

        if (Profile.FastClockState == FastClockStateEnum.On) {
            await SendCommandAsync(JsonSerializer.Serialize(new { type = "time" }));
        }
    }

    // -------------------- Outbound helpers (JMRI JSON) --------------------
    public Task SetTurnoutStateAsync(string turnoutId, bool thrown) {
        var cmd = BuildJmriMessage("turnout", "post", new Dictionary<string, object> {
            { "type", "turnout" }, { "name", turnoutId }, { "state", thrown ? 4 : 2 }
        });
        return SendCommandAsync(cmd);
    }

    public Task SetSignalAppearanceAsync(string signalId, string appearance) {
        // Many installs use signal masts; your prior sender targeted "signalMast"
        var cmd = BuildJmriMessage("signalMast", "post", new Dictionary<string, object> {
            { "name", signalId }, { "set", appearance }
        });
        return SendCommandAsync(cmd);
    }

    public Task SetRouteStateAsync(string routeId, bool active) {
        var cmd = BuildJmriMessage("route", "post", new Dictionary<string, object> {
            { "type", "route" }, { "name", routeId }, { "state", active ? 2 : 4 }
        });
        return SendCommandAsync(cmd);
    }

    public Task SetSensorStateAsync(string sensorId, bool active) {
        var cmd = BuildJmriMessage("sensor", "post", new Dictionary<string, object> {
            { "type", "sensor" }, { "name", sensorId }, { "state", active ? 2 : 4 }
        });
        return SendCommandAsync(cmd);
    }

    public Task SetLightStateAsync(string lightId, bool active) {
        var cmd = BuildJmriMessage("light", "post", new Dictionary<string, object> {
            { "type", "light" }, { "name", lightId }, { "state", active ? 2 : 4 }
        });
        return SendCommandAsync(cmd);
    }

    public Task SetBlockAllocatedAsync(string blockId, bool allocated) {
        var cmd = BuildJmriMessage("block", "post", new Dictionary<string, object> {
            { "type", "block" }, { "name", blockId }, { "state", allocated ? 2 : 4 }
        });
        return SendCommandAsync(cmd);
    }

    public Task SetBlockValueAsync(string blockId, string value) {
        var cmd = BuildJmriMessage("block", "post", new Dictionary<string, object> {
            { "type", "block" }, { "name", blockId }, { "state", value }
        });
        return SendCommandAsync(cmd);
    }

    private static string BuildJmriMessage(string type, string? method, Dictionary<string, object>? parameters) {
        var msg = new Dictionary<string, object?> { ["type"] = type };
        if (method is not null) msg["method"] = method;
        if (parameters is { Count: > 0 }) msg["data"] = parameters;
        return JsonSerializer.Serialize(msg);
    }

    private Task SendCommandAsync(string? json) => (_ws is { State: WebSocketState.Open } && !string.IsNullOrEmpty(json))
        ? SendMessageAsync(json)
        : Task.CompletedTask;

    private async Task SendMessageAsync(string message) {
        try {
            if (_ws?.State == WebSocketState.Open) {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        } catch (Exception ex) {
            Debug.WriteLine($"JMRI send failed: {ex.Message}");
            throw;
        }
    }

    // -------------------- Utilities --------------------
    private void OnConnectionStateChanged(string message,
        [CallerMemberName] string member = "unknown", [CallerLineNumber] int line = 0) {
        OnClientMessage($"{message}", DccClientOperation.System, DccClientMessageType.System);
    }

    private void ResetCaches() {
        _turnouts.Clear();
        _signals.Clear();
        _routes.Clear();
        _sensors.Clear();
        _blocks.Clear();
        _lights.Clear();
    }

    private void CloseSocket() {
        try {
            _ws?.Abort();
        } catch { /* ignore */
        }
        try {
            _ws?.Dispose();
        } catch { /* ignore */
        }
        _ws = null;
    }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        try {
            _ = DisconnectAsync();
        } catch { /* ignore */
        }
    }
}