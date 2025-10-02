using System.Data;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DCCPanelController.Clients.Discovery;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Models.DataModel;

// WiThrottle bits you already have:
using DCCPanelController.Clients.WiThrottle.Client;
using DCCPanelController.Clients.WiThrottle.Commands;
using DCCPanelController.Clients.WiThrottle.Events; // MessageProcessor, etc.
// TurnoutEvent, RouteEvent, ...
using DCCPanelController.Helpers; // TurnoutCommand, RouteCommand

namespace DCCPanelController.Clients.WiThrottle {
    /// <summary>
    /// Async, proxy-free WiThrottle client. Implements IDccClient directly and uses DccClientBase to update Profile.
    /// </summary>
    public sealed partial class WiThrottleDccClient : DccClientBase, IDccClient, IDisposable {
        private readonly WiThrottleSettings _settings;

        private TcpClient?                  _tcp;
        private NetworkStream?              _stream;
        private CancellationTokenSource?    _cts;
        private Task?                       _recvLoop;
        private TaskCompletionSource<bool>? _openedTcs;

        private readonly byte[]               _buf = new byte[4096];
        private readonly StringBuilder        _sb  = new();
        private          System.Timers.Timer? _heartbeat;

        public WiThrottleDccClient(Profile profile, IDccClientSettings settings) : base(profile) {
            _settings = settings as WiThrottleSettings
                     ?? throw new InvalidCastException("Incorrect Settings Type provided for WiThrottle");
        }

        public DccClientType Type => DccClientType.WiThrottle;

        // ---- Lifecycle ------------------------------------------------------

        public async Task<IResult> ConnectAsync() {
            await DisconnectAsync(); // clean slate

            // 1) Try autodetect; your UpdateAutomaticSettings() already does this
            // var settings = await UpdateAutomaticSettings();
            // if (settings.IsFailure) {
            //     State = DccClientState.Disconnected;
            //     OnClientMessage("Could not auto-detect connection settings.");
            //     return Result.Fail("Could not auto-detect connection settings.");
            // }

            // 2) Hard probe: fail-fast with no retries if we can't open now
            var probe = await ValidateConnectionAsync();
            if (probe.IsFailure) {
                State = DccClientState.Error;
                OnClientMessage($"Initial connect failed: {probe.Message}", DccClientOperation.System, DccClientMessageType.Error);
                return Result.Fail(probe.Message ?? "Initial connect failed");
            }

            State = DccClientState.Initialising;
            OnClientMessage($"Connecting to WiThrottle {_settings.Address}:{_settings.Port}...");

            // 3) Only start the reconnect loop after we know we can connect
            _cts = new CancellationTokenSource();
            _ = RunReconnectLoopAsync(
                connectOnce: ConnectAndListenAsync,
                isDisposed: () => false,
                maxRetries: _settings.MaxRetries <= 0 ? int.MaxValue : _settings.MaxRetries,
                initialDelay: TimeSpan.FromMilliseconds(_settings.InitialBackoffMs <= 0 ? 1000 : _settings.InitialBackoffMs),
                multiplier: _settings.BackoffMultiplier <= 1 ? 1 : _settings.BackoffMultiplier,
                ct: _cts.Token
            );
            return Result.Ok();
        }

        private async Task ConnectAndListenAsync(CancellationToken ct) {
            _openedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            // Update the address and port if Automatic Settings is turned on
            // --------------------------------------------------------------
            var result = await UpdateAutomaticSettings();
            if (result.IsFailure) {
                State = DccClientState.Error;
                OnClientMessage("Could not auto-detect connection settings.");
                return;
            }
            
            // (re)open transport
            // --------------------------------------------------------------
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_settings.Address, _settings.Port, ct);
            _stream = _tcp.GetStream();

            // wake up & set Connected
            await SendRawAsync($"N{_settings.Name}");
            await SendRawAsync($"HU{Guid.NewGuid()}");
            await SendRawAsync("*+");

            // start to receive; don’t set Connected yet
            var recvTask = ReceiveLoopAsync(ct);

            // wait up to a few seconds for *any* inbound to prove liveness
            var opened = await Task.WhenAny(_openedTcs.Task, Task.Delay(3000, ct)) == _openedTcs.Task;
            if (opened) {
                State = DccClientState.Connected;
                OnClientMessage("WiThrottle connected");
            }

            // wait for loop (throws on close to trigger backoff)
            await recvTask;
            throw new IOException("WiThrottle socket closed");
        }

        public async Task<IResult> DisconnectAsync() {
            try {
                await _cts?.CancelAsync()!;
                if (_stream is { CanWrite: true }) {
                    try {
                        await SendRawAsync("*-");
                        await SendRawAsync("Q");
                    } catch { /* best-effort */
                    }
                }

                // Wait briefly for the loop to exit cleanly
                if (_recvLoop is { }) {
                    var completed = await Task.WhenAny(_recvLoop, Task.Delay(1000));
                }

                CloseTransport();
                _recvLoop = null;

                State = DccClientState.Disconnected;
                OnClientMessage("WiThrottle disconnected");
                return Result.Ok();
            } catch (Exception ex) {
                return Result.Fail(ex, "WiThrottle disconnect failed");
            }
        }

        private void CloseTransport() {
            try {
                StopHeartbeat();
            } catch { /*ignore*/
            }
            try {
                _stream?.Close();
            } catch { /*ignore*/
            }
            try {
                _tcp?.Close();
            } catch { /*ignore*/
            }
            _stream = null;
            _tcp = null;
        }

        public async Task<IResult> UpdateAutomaticSettings() {
            if (_settings.SetAutomatically) {
                var auto = await GetAutomaticConnectionDetailsAsync();
                if (auto.IsFailure || auto.Value is null || auto.Value is not WiThrottleSettings) {
                    State = DccClientState.Disconnected;
                    OnClientMessage("Could not automatically set connection details. Is WiThrottle service running?");
                    return Result.Fail(auto.Message ?? "Autodetect failed.");
                }
                if (auto.Value is WiThrottleSettings settings) {
                    _settings.Address = settings.Address;
                    _settings.Port = settings.Port;
                    OnClientMessage($"Found server at {_settings.Address}:{_settings.Port}");
                }
            }
            return Result.Ok();
        }
        
        public async Task<IResult> ValidateConnectionAsync() {
            try {
                var result = await UpdateAutomaticSettings();
                if (result.IsFailure) {
                    OnClientMessage("Could not auto-detect connection settings.");
                    return Result.Fail("Could not auto-detect connection settings.");
                }

                using var tcp = new TcpClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await tcp.ConnectAsync(_settings.Address, _settings.Port, cts.Token);
                await using var stream = tcp.GetStream();

                if (stream is { CanWrite: true }) {
                    async Task SendAsync(string s, NetworkStream sendStream) {
                        if (!s.EndsWith('\n')) s += "\n";
                        var bytes = Encoding.UTF8.GetBytes(s);
                        await sendStream.WriteAsync(bytes.AsMemory(0, bytes.Length), cts.Token);
                    }

                    await SendAsync($"N{_settings.Name}", stream);
                    await SendAsync($"HU{Guid.NewGuid()}", stream);
                    await SendAsync("*+", stream);

                    var buf = new byte[1024];
                    var read = await stream.ReadAsync(buf.AsMemory(0, buf.Length), cts.Token);
                    if (read > 0) return Result.Ok("WiThrottle responded"); // any line suffices for probe
                }
                return Result.Fail("WiThrottle no response");
            } catch (Exception ex) {
                return Result.Fail(ex, "WiThrottle probe failed");
            }
        }

        public Task<IResult> SetAutomaticSettingsAsync() => Task.FromResult<IResult>(Result.Ok("WiThrottle settings OK"));

        public Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null) {
            // WiThrottle is largely push-driven. If you add a query cmd, send it here.
            OnClientMessage("WiThrottle refresh requested");
            return Task.FromResult<IResult>(Result.Ok());
        }

        // ---- Commands (supported: Turnouts & Routes per your proxy) ---------

        public async Task<IResult> SendTurnoutCmdAsync(Turnout turnout, bool thrown) {
            if (State != DccClientState.Connected || _stream is null) return Result.Fail("Not connected to WiThrottle server");
            if (string.IsNullOrEmpty(turnout.Id)) return Result.Fail("Invalid Turnout Id provided.");

            try {
                // Reuse your existing command object; it exposes Command string
                var cmd = new TurnoutCommand(turnout.Id, thrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed);
                await SendRawAsync(cmd.Command);
                OnClientMessage($"Setting turnout {turnout.Name}({turnout.Id}) to {(thrown ? "THROWN" : "CLOSED")}", DccClientOperation.Turnout, DccClientMessageType.Outbound);
                return Result.Ok();
            } catch (Exception ex) {
                return Result.Fail(ex, "Failed to send turnout command to WiThrottle server");
            }
        }

        public async Task<IResult> SendRouteCmdAsync(Route route, bool active) {
            if (State != DccClientState.Connected || _stream is null) return Result.Fail("Not connected to WiThrottle server");
            if (string.IsNullOrEmpty(route.Id)) return Result.Fail("Invalid Route Id provided.");

            try {
                // Your proxy sends a RouteCommand without an active flag; mirror that.
                var cmd = new RouteCommand(route.Id);
                await SendRawAsync(cmd.Command);
                OnClientMessage($"Setting route {route.Name}({route.Id}) to {(active ? "ACTIVE" : "INACTIVE")}",
                    DccClientOperation.Route, DccClientMessageType.Outbound);
                return Result.Ok();
            } catch (Exception ex) {
                return Result.Fail(ex, "Failed to send route command to WiThrottle server");
            }
        }

        public Task<IResult> SendSignalCmdAsync(Signal signal, SignalAspectEnum aspect) => Task.FromResult<IResult>(Result.Fail("Signal commands are not supported by WiThrottle."));

        public Task<IResult> SendLightCmdAsync(Light light, bool isActive) => Task.FromResult<IResult>(Result.Fail("Light commands are not supported by WiThrottle."));

        public Task<IResult> SendBlockCmdAsync(Block block, bool isOccupied) => Task.FromResult<IResult>(Result.Fail("Block commands are not supported by WiThrottle."));

        public Task<IResult> SendSensorCmdAsync(Sensor sensor, bool isOccupied) => Task.FromResult<IResult>(Result.Fail("Sensor commands are not supported by WiThrottle."));

        // ---- Discovery ------------------------------------------------------

        public Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() => DccClientFactory.FindServices(DccClientType.WiThrottle);

        public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
            var found = await FindAvailableServicesAsync();
            if (found.IsFailure || found.Value is null || found.Value.Count == 0)
                return Result<IDccClientSettings?>.Fail("No available servers could be found.");

            foreach (var server in found.Value) {
                if (server.Address.ToString() != "0.0.0.0" && server.Address.ToString() != "127.0.0.1") {
                    var rawSettings = new WiThrottleSettings {
                        Address = server.Address.ToString(),
                        Port = server.Port,
                    };
                    return Result<IDccClientSettings?>.Ok(rawSettings);
                }
            }
            return Result<IDccClientSettings?>.Fail("No available servers could be found.");
        }

        public void Dispose() => _cts?.Cancel();

        // ---- Receive/parse/map ---------------------------------------------

        private async Task ReceiveLoopAsync(CancellationToken ct) {
            try {
                while (!ct.IsCancellationRequested && _tcp is { Connected: true } && _stream is not null) {
                    if (!_stream.CanRead) break;

                    var read = await _stream.ReadAsync(_buf.AsMemory(0, _buf.Length), ct);
                    if (read <= 0) break;

                    _sb.Append(Encoding.ASCII.GetString(_buf, 0, read));

                    foreach (var msg in SplitMessages(_sb)) {
                        ProcessInbound(msg);
                    }
                }
            } catch (OperationCanceledException) { /* normal */
            } catch (Exception ex) {
                OnClientMessage($"WiThrottle read error: {ex.Message}", DccClientOperation.System, DccClientMessageType.Error);
            } finally {
                CloseTransport();
                if (!ct.IsCancellationRequested) {
                    State = DccClientState.Disconnected;
                    OnClientMessage("Disconnecting from WiThrottle server");
                }
            }
        }

        private void ProcessInbound(string message) {
            _openedTcs?.TrySetResult(true);
            var clientMsg = MessageProcessor.Interpret(message);

            switch (clientMsg.GetType().Name) {
                case"MsgQuit":
                    OnClientMessage("WiThrottle server requested quit");
                    _ = DisconnectAsync();
                    return;

                case"MsgHeartbeat":
                    var hbProp = clientMsg.GetType().GetProperty("HeartbeatSeconds");
                    var hbSecs = (int?)(hbProp?.GetValue(clientMsg)) ?? 15;
                    RestartHeartbeat(hbSecs);
                    OnClientMessage($"WiThrottle heartbeat={hbSecs}s");
                break;

                default:
                    var eventsProp = clientMsg.GetType().GetProperty("FoundEvents");
                    if (eventsProp?.GetValue(clientMsg) is System.Collections.IEnumerable events) {
                        foreach (var ev in events) MapEvent(ev as IClientEvent);
                    }
                break;
            }
        }

        private void MapEvent(IClientEvent? ev) {
            if (ev is null) return;

            switch (ev) {
                case MessageEvent m:
                    OnClientMessage(m.Value, DccClientOperation.System, DccClientMessageType.Inbound);
                break;

                case ConnectionEvent c:
                    State = c.State switch {
                        ConnectionState.Open       => DccClientState.Connected,
                        ConnectionState.Connecting => DccClientState.Initialising,
                        _                          => DccClientState.Disconnected,
                    };
                    OnClientMessage($"WiThrottle connection: {c.State}");
                break;

                case TurnoutEvent t:
                    UpdateTurnout(t.SystemName, t.UserName,
                        t.StateEnum == TurnoutStateEnum.Thrown
                            ? Models.DataModel.Entities.TurnoutStateEnum.Thrown
                            : Models.DataModel.Entities.TurnoutStateEnum.Closed);
                    OnClientMessage($"Turnout Change Event: {t.SystemName}=>{t.State}",
                        DccClientOperation.Turnout, DccClientMessageType.Inbound);
                break;

                case RouteEvent r:
                    UpdateRoute(r.SystemName, r.UserName,
                        r.StateEnum == RouteStateEnum.Active
                            ? Models.DataModel.Entities.RouteStateEnum.Active
                            : Models.DataModel.Entities.RouteStateEnum.Inactive);
                    OnClientMessage($"Route Change Event: {r.SystemName}=>{r.State}",
                        DccClientOperation.Route, DccClientMessageType.Inbound);
                break;

                case FastClockEvent fc:
                    OnClientMessage($"FastClock: {fc.Time:T}", DccClientOperation.System, DccClientMessageType.Inbound);
                break;

                case RosterEvent _:
                    OnClientMessage("Roster update", DccClientOperation.System, DccClientMessageType.Inbound);
                break;

                default:
                    OnClientMessage($"WiThrottle event {ev.GetType().Name}: {ev}",
                        DccClientOperation.System, DccClientMessageType.Inbound);
                break;
            }
        }

        // ---- Send / Heartbeat helpers --------------------------------------

        private async Task SendRawAsync(string command) {
            if (_stream is null || !_stream.CanWrite) throw new IOException("Stream not writable");
            if (!command.EndsWith('\n')) command += "\n";
            var data = Encoding.UTF8.GetBytes(command);
            await _stream.WriteAsync(data, 0, data.Length, _cts?.Token ?? CancellationToken.None);

            //OnClientMessage($"WiThrottle Send: {command.TrimEnd("\n")}",DccClientOperation.System, DccClientMessageType.Outbound);
        }

        private void RestartHeartbeat(int secs) {
            StopHeartbeat();
            _heartbeat = new System.Timers.Timer(Math.Max(1, secs) * 1000);
            _heartbeat.AutoReset = true;
            _heartbeat.Elapsed += async (_, _) => {
                try {
                    await SendRawAsync("*");
                } catch { /* ignored */
                }
            };
            _heartbeat.Start();
        }

        private void StopHeartbeat() {
            if (_heartbeat is null) return;
            _heartbeat.Stop();
            _heartbeat.Dispose();
            _heartbeat = null;
        }

        private static IEnumerable<string> SplitMessages(StringBuilder sb) {
            // Split on \r?\n and leave the last (possibly incomplete) fragment in the buffer
            var text = sb.ToString();
            var parts = LineSplit().Split(text);
            sb.Clear();
            if (parts.Length == 0) yield break;
            for (int i = 0; i < parts.Length - 1; i++)
                if (!string.IsNullOrEmpty(parts[i]))
                    yield return parts[i];
            sb.Append(parts[^1]);
        }

        [GeneratedRegex(@"\r?\n")]
        private static partial Regex LineSplit();
    }
}