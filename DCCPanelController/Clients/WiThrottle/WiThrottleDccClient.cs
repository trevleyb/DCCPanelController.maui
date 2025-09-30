using System.Data;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DCCPanelController.Clients.Discovery;
using DCCPanelController.Models.DataModel;

// WiThrottle bits you already have:
using DCCPanelController.Clients.WiThrottle.Client;        // MessageProcessor, etc.
using DCCPanelController.Clients.WiThrottle.Client.Events; // TurnoutEvent, RouteEvent, ...
using DCCPanelController.Clients.WiThrottle.Client.Commands;
using DCCPanelController.Helpers; // TurnoutCommand, RouteCommand

namespace DCCPanelController.Clients.WiThrottle {
    /// <summary>
    /// Async, proxy-free WiThrottle client. Implements IDccClient directly and uses DccClientBase to update Profile.
    /// </summary>
    public sealed partial class WiThrottleDccClient : DccClientBase, IDccClient, IDisposable {
        private readonly WiThrottleSettings _settings;

        private TcpClient?               _tcp;
        private NetworkStream?           _stream;
        private CancellationTokenSource? _cts;
        private Task?                    _recvLoop;

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
            _cts = new CancellationTokenSource();

            try {
                Status = DccClientStatus.Initialising;
                IsConnected = false;
                OnClientMessage($"Connecting to WiThrottle {_settings.Address}:{_settings.Port}...");

                _tcp = new TcpClient();
                using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
                connectCts.CancelAfter(TimeSpan.FromSeconds(10));
                await _tcp.ConnectAsync(_settings.Address, _settings.Port, connectCts.Token);

                _stream = _tcp.GetStream();

                // Wake-up handshake (same as your current client)
                await SendRawAsync($"N{_settings.Name}");  // Name message
                await SendRawAsync($"HU{Guid.NewGuid()}"); // Hardware/UUID
                await SendRawAsync("*+");                  // Request heartbeat / capabilities  :contentReference[oaicite:1]{index=1}

                IsConnected = true;
                Status = DccClientStatus.Connected;
                OnClientMessage("WiThrottle connected");

                _recvLoop = Task.Run(() => ReceiveLoopAsync(_cts!.Token), connectCts.Token);
                _ = _recvLoop.ContinueWith(t => {
                    var msg = t.Exception?.GetBaseException().Message ?? "unknown error";
                    OnClientMessage($"WiThrottle receive loop faulted: {msg}", DccClientOperation.System, DccClientMessageType.Error);
                }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
                return Result.Ok("Connected");
            } catch (Exception ex) {
                await DisconnectAsync();
                return Result.Fail(ex, "WiThrottle connect failed");
            }
        }

        public async Task<IResult> DisconnectAsync() {
            try {
                _cts?.Cancel();

                // Try graceful shutdown if we still have a writable stream
                if (_stream is { CanWrite: true }) {
                    try {
                        await SendRawAsync("*-");
                        await SendRawAsync("Q");
                    } catch { /* best-effort */
                    }
                }

                // Wait briefly for the loop to exit cleanly
                if (_recvLoop is not null) {
                    var completed = await Task.WhenAny(_recvLoop, Task.Delay(1000));
                }

                CloseTransport();
                _recvLoop = null;

                IsConnected = false;
                Status = DccClientStatus.Disconnected;
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

        public Task<IResult> ValidateConnectionAsync() => Task.FromResult<IResult>(IsConnected ? Result.Ok("WiThrottle connected") : Result.Fail("WiThrottle not connected"));

        public Task<IResult> SetAutomaticSettingsAsync() => Task.FromResult<IResult>(Result.Ok("WiThrottle settings OK"));

        public Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null) {
            // WiThrottle is largely push-driven. If you add a query cmd, send it here.
            OnClientMessage("WiThrottle refresh requested");
            return Task.FromResult<IResult>(Result.Ok());
        }

        // ---- Commands (supported: Turnouts & Routes per your proxy) ---------

        public async Task<IResult> SendTurnoutCmdAsync(Turnout turnout, bool thrown) {
            if (!IsConnected || _stream is null) return Result.Fail("Not connected to WiThrottle server");
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
            if (!IsConnected || _stream is null) return Result.Fail("Not connected to WiThrottle server");
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

        public Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() => Task.FromResult(Result<IDccClientSettings?>.Ok(_settings) as IResult<IDccClientSettings?>);

        public void Dispose() => _cts?.Cancel();

        // ---- Receive/parse/map ---------------------------------------------

        private async Task ReceiveLoopAsync(CancellationToken ct) {
            try {
                // kick a heartbeat request (matches your existing behavior)
                await SendRawAsync("*+");

                while (!ct.IsCancellationRequested && _tcp is { Connected: true } && _stream is not null) {
                    if (!_stream.CanRead) break;

                    int read = await _stream.ReadAsync(_buf.AsMemory(0, _buf.Length), ct);
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
                IsConnected = false;
                Status = DccClientStatus.Disconnected;
                OnClientMessage("Disconnecting from WiThrottle server");
                await DisconnectAsync();
            }
        }

        private void ProcessInbound(string message) {
            // Your parser: returns MsgHeartbeat/MsgQuit/... and exposes FoundEvents
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
                        foreach (var ev in events)
                            MapEvent(ev as IClientEvent);
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
                    Status = c.State switch {
                        ConnectionState.Open       => DccClientStatus.Connected,
                        ConnectionState.Connecting => DccClientStatus.Initialising,
                        _                          => DccClientStatus.Disconnected,
                    };
                    IsConnected = Status == DccClientStatus.Connected;
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
        }

        private void RestartHeartbeat(int secs) {
            StopHeartbeat();
            _heartbeat = new System.Timers.Timer(Math.Max(1, secs) * 1000);
            _heartbeat.AutoReset = true;
            _heartbeat.Elapsed += async (_, _) => {
                try {
                    await SendRawAsync("*");
                } catch { /* ignored */ }
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