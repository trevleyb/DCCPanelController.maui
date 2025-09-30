using DCCPanelController.Clients.Discovery;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Clients.Simulator {
    /// <summary>
    /// Protocol-like simulator:
    /// - Handshake ("hello") then heartbeats
    /// - Fast clock ticks (sim time)
    /// - Random flips + occasional bursts (like relist/subscription dumps)
    /// - Optional failure injection (timed disconnect)
    /// - Deterministic with Seed if provided in settings
    /// </summary>
    public sealed class SimulatorDccClient : DccClientBase, IDccClient, IDisposable {
        private readonly SimulatorSettings _settings;

        // timers
        private System.Timers.Timer? _heartbeat;
        private System.Timers.Timer? _randomFlips;
        private System.Timers.Timer? _bursts;
        private System.Timers.Timer? _fastClock;
        private System.Timers.Timer? _failureInjector;

        // random & sim time
        private readonly Random   _rand;
        private          DateTime _simClock;
        private          double   _clockRate; // 1.0 = real time, 10.0 = 10x, etc.

        // entity seeding
        private const int SeedCount = 4;

        // cached knobs (read from settings if present, else defaults)
        private readonly int _tickMs;
        private readonly int _heartbeatSecs;
        private readonly int _burstEveryMs;
        private readonly int _burstSizeMin;
        private readonly int _burstSizeMax;
        private readonly int _disconnectEveryMs;

        public SimulatorDccClient(Profile profile, IDccClientSettings settings) : base(profile) {
            _settings = settings as SimulatorSettings
                     ?? throw new InvalidCastException("Incorrect Settings Type provided for Simulator");

            // read optional knobs from settings via reflection (no breaking changes to your class)
            _tickMs = GetInt(_settings, "TickMs", 1000);
            _heartbeatSecs = GetInt(_settings, "HeartbeatSeconds", 15);
            _burstEveryMs = GetInt(_settings, "BurstEveryMs", 12_000);
            _burstSizeMin = GetInt(_settings, "BurstSizeMin", 3);
            _burstSizeMax = GetInt(_settings, "BurstSizeMax", 8);
            _disconnectEveryMs = GetInt(_settings, "DisconnectEveryMs", 0); // 0 = disabled
            var clockRateD = GetDouble(_settings, "ClockRate", 12.0);       // 12x sim time by default
            var seed = GetInt(_settings, "Seed", Environment.TickCount);

            _clockRate = clockRateD <= 0 ? 1.0 : clockRateD;
            _rand = new Random(seed);
        }

        public DccClientType Type => DccClientType.Simulator;

        // -------------------- Lifecycle --------------------

        public Task<IResult> ConnectAsync() {
            Status = DccClientStatus.Initialising;

            // Reset view and seed a small layout
            Profile.RefreshAll();

            //SeedEntities();

            // "Hello" after a brief handshake delay
            Task.Run(async () => {
                await Task.Delay(250);
                OnClientMessage($"SIM HELLO: heartbeat={_heartbeatSecs}s; clockRate={_clockRate:0.##}x",
                    DccClientOperation.System, DccClientMessageType.Inbound);

                StartHeartbeat();
                StartFastClock();
                StartRandomFlips();
                StartBursts();
                StartFailureInjection();

                Status = DccClientStatus.Connected;
                OnClientMessage("Simulator connected");
            });

            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> DisconnectAsync() {
            StopTimers();

            Status = DccClientStatus.Disconnected;
            OnClientMessage("Simulator disconnected");
            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> ValidateConnectionAsync() => Task.FromResult<IResult>(Status == DccClientStatus.Connected ? Result.Ok("Simulator connected") : Result.Fail("Simulator not connected"));

        public Task<IResult> SetAutomaticSettingsAsync() => Task.FromResult<IResult>(Result.Ok("Simulator settings OK"));

        public Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null) {
            // Simulate a relist/subscription replay by sending a burst for each type
            SendBurst();
            OnClientMessage("Simulator: full relist requested", DccClientOperation.System, DccClientMessageType.Inbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        // -------------------- Commands --------------------

        public Task<IResult> SendTurnoutCmdAsync(Turnout t, bool thrown) {
            if (string.IsNullOrWhiteSpace(t.Id) || string.IsNullOrWhiteSpace(t.Name))
                return Task.FromResult<IResult>(Result.Fail("Invalid Turnout Id/Name"));

            UpdateTurnout(t.Id, t.Name, thrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed);
            OnClientMessage($"Sim TX: turnout {t.Id} {(thrown ? "THROWN" : "CLOSED")}",
                DccClientOperation.Turnout, DccClientMessageType.Outbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> SendRouteCmdAsync(Route r, bool active) {
            if (string.IsNullOrWhiteSpace(r.Id) || string.IsNullOrWhiteSpace(r.Name))
                return Task.FromResult<IResult>(Result.Fail("Invalid Route Id/Name"));

            UpdateRoute(r.Id, r.Name, active ? RouteStateEnum.Active : RouteStateEnum.Inactive);
            OnClientMessage($"Sim TX: route {r.Id} {(active ? "ACTIVE" : "INACTIVE")}",
                DccClientOperation.Route, DccClientMessageType.Outbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> SendSignalCmdAsync(Signal s, SignalAspectEnum aspect) {
            if (string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Name))
                return Task.FromResult<IResult>(Result.Fail("Invalid Signal Id/Name"));

            UpdateSignal(s.Id, s.Name, aspect);
            OnClientMessage($"Sim TX: signal {s.Id} → {aspect}",
                DccClientOperation.Signal, DccClientMessageType.Outbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> SendLightCmdAsync(Light l, bool isActive) {
            if (string.IsNullOrWhiteSpace(l.Id) || string.IsNullOrWhiteSpace(l.Name))
                return Task.FromResult<IResult>(Result.Fail("Invalid Light Id/Name"));

            UpdateLight(l.Id, l.Name, isActive);
            OnClientMessage($"Sim TX: light {l.Id} {(isActive ? "ON" : "OFF")}",
                DccClientOperation.Light, DccClientMessageType.Outbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> SendBlockCmdAsync(Block b, bool isOccupied) {
            if (string.IsNullOrWhiteSpace(b.Id) || string.IsNullOrWhiteSpace(b.Name))
                return Task.FromResult<IResult>(Result.Fail("Invalid Block Id/Name"));

            UpdateBlock(b.Id, b.Name, isOccupied);
            OnClientMessage($"Sim TX: block {b.Id} {(isOccupied ? "OCCUPIED" : "FREE")}",
                DccClientOperation.Block, DccClientMessageType.Outbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        public Task<IResult> SendSensorCmdAsync(Sensor s, bool isOccupied) {
            if (string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Name))
                return Task.FromResult<IResult>(Result.Fail("Invalid Sensor Id/Name"));

            UpdateSensor(s.Id, s.Name, isOccupied);
            OnClientMessage($"Sim TX: sensor {s.Id} {(isOccupied ? "ON" : "OFF")}",
                DccClientOperation.Sensor, DccClientMessageType.Outbound);
            return Task.FromResult<IResult>(Result.Ok());
        }

        // -------------------- Discovery (dummy) --------------------

        public Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() {
            var dummy = new DiscoveredService {
                InstanceName = "Simulator",
                FriendlyName = "Local Simulator",
                HostName = "localhost",
                Port = 0,
                ServiceType = "simulator",
                Addresses = [],
                TxtRecords = new Dictionary<string, string> {
                    { "description", "DCC Panel Simulator" },
                    { "clockRate", _clockRate.ToString("0.##") },
                },
            };
            return Task.FromResult<IResult<List<DiscoveredService>>>(Result<List<DiscoveredService>>.Ok(new List<DiscoveredService> { dummy }));
        }

        public Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() => Task.FromResult<IResult<IDccClientSettings?>>(Result<IDccClientSettings?>.Ok(_settings));

        // -------------------- Internals --------------------

        private void StartHeartbeat() {
            _heartbeat = new System.Timers.Timer(Math.Max(1, _heartbeatSecs) * 1000);
            _heartbeat.Elapsed += (_, _) => {
                OnClientMessage("SIM HB", DccClientOperation.System, DccClientMessageType.Inbound);

                // Tiny chance to flip something exactly on the heartbeat
                if (_rand.Next(5) == 0) FlipRandomOnce();
            };
            _heartbeat.AutoReset = true;
            _heartbeat.Start();
        }

        private void StartFastClock() {
            _simClock = DateTime.Today.AddHours(9); // arbitrary starting time
            _fastClock = new System.Timers.Timer(1000);
            _fastClock.AutoReset = true;
            _fastClock.Elapsed += (_, _) => {
                // advance sim time by _clockRate seconds each real second
                _simClock = _simClock.AddSeconds(_clockRate);
                OnClientMessage($"FastClock: {_simClock:HH:mm:ss}", DccClientOperation.System, DccClientMessageType.Inbound);
            };
            _fastClock.Start();
        }

        private void StartRandomFlips() {
            _randomFlips = new System.Timers.Timer(_tickMs);
            _randomFlips.AutoReset = true;
            _randomFlips.Elapsed += (_, _) => FlipRandomOnce();
            _randomFlips.Start();
        }

        private void StartBursts() {
            if (_burstEveryMs <= 0) return;

            _bursts = new System.Timers.Timer(_burstEveryMs);
            _bursts.AutoReset = true;
            _bursts.Elapsed += (_, _) => SendBurst();
            _bursts.Start();
        }

        private void StartFailureInjection() {
            if (_disconnectEveryMs <= 0) return;

            _failureInjector = new System.Timers.Timer(_disconnectEveryMs);
            _failureInjector.AutoReset = true;
            _failureInjector.Elapsed += async (_, _) => {
                OnClientMessage("SIM: injected disconnect", DccClientOperation.System, DccClientMessageType.Error);
                await DisconnectAsync();
            };
            _failureInjector.Start();
        }

        private void StopTimers() {
            Stop(_heartbeat);
            _heartbeat = null;
            Stop(_fastClock);
            _fastClock = null;
            Stop(_randomFlips);
            _randomFlips = null;
            Stop(_bursts);
            _bursts = null;
            Stop(_failureInjector);
            _failureInjector = null;

            static void Stop(System.Timers.Timer? t) {
                if (t is null) return;
                try {
                    t.Stop();
                    t.Dispose();
                } catch { /* no-op */
                }
            }
        }

        public void SeedEntities() {
            for (var i = 101; i < 101 + SeedCount; i++)
                UpdateTurnout($"NT{i}", $"Turnout {i}", _rand.Next(2) == 0 ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown);

            for (var i = 201; i < 201 + SeedCount; i++)
                UpdateRoute($"RT{i}", $"Route {i}", _rand.Next(2) == 0 ? RouteStateEnum.Active : RouteStateEnum.Inactive);

            for (var i = 301; i < 301 + SeedCount; i++)
                UpdateSensor($"SN{i}", $"Sensor {i}", _rand.Next(2) == 0);

            for (var i = 401; i < 401 + SeedCount; i++)
                UpdateLight($"LT{i}", $"Light {i}", _rand.Next(2) == 0);

            for (var i = 501; i < 501 + SeedCount; i++)
                UpdateBlock($"BK{i}", $"Block {i}", _rand.Next(2) == 0);

            for (var i = 601; i < 601 + SeedCount; i++)
                UpdateSignal($"SG{i}", $"Signal {i}", _rand.Next(3) switch {
                    0 => SignalAspectEnum.Clear,
                    1 => SignalAspectEnum.Approach,
                    _ => SignalAspectEnum.Stop
                });

            OnClientMessage("SIM: initial relist complete", DccClientOperation.System, DccClientMessageType.Inbound);
        }

        private void FlipRandomOnce() {
            if (Status != DccClientStatus.Connected) return;

            var pick = _rand.Next(6);
            switch (pick) {
                case 0: {
                    var (id, name) = Pick("NT", 101, SeedCount, "Turnout");
                    var thrown = _rand.Next(2) == 0;
                    UpdateTurnout(id, name, thrown ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed);
                    OnClientMessage($"Sim flip: {id} {(thrown ? "THROWN" : "CLOSED")}", DccClientOperation.Turnout, DccClientMessageType.Inbound);
                    break;
                }

                case 1: {
                    var (id, name) = Pick("RT", 201, SeedCount, "Route");
                    var active = _rand.Next(2) == 0;
                    UpdateRoute(id, name, active ? RouteStateEnum.Active : RouteStateEnum.Inactive);
                    OnClientMessage($"Sim flip: {id} {(active ? "ACTIVE" : "INACTIVE")}", DccClientOperation.Route, DccClientMessageType.Inbound);
                    break;
                }

                case 2: {
                    var (id, name) = Pick("SN", 301, SeedCount, "Sensor");
                    var occ = _rand.Next(2) == 0;
                    UpdateSensor(id, name, occ);
                    OnClientMessage($"Sim flip: {id} {(occ ? "ON" : "OFF")}", DccClientOperation.Sensor, DccClientMessageType.Inbound);
                    break;
                }

                case 3: {
                    var (id, name) = Pick("LT", 401, SeedCount, "Light");
                    var on = _rand.Next(2) == 0;
                    UpdateLight(id, name, on);
                    OnClientMessage($"Sim flip: {id} {(on ? "ON" : "OFF")}", DccClientOperation.Light, DccClientMessageType.Inbound);
                    break;
                }

                case 4: {
                    var (id, name) = Pick("BK", 501, SeedCount, "Block");
                    var occ = _rand.Next(2) == 0;
                    UpdateBlock(id, name, occ);
                    OnClientMessage($"Sim flip: {id} {(occ ? "OCCUPIED" : "FREE")}", DccClientOperation.Block, DccClientMessageType.Inbound);
                    break;
                }

                case 5: {
                    var (id, name) = Pick("SG", 601, SeedCount, "Signal");
                    var aspect = _rand.Next(3) switch {
                        0 => SignalAspectEnum.Clear,
                        1 => SignalAspectEnum.Approach,
                        _ => SignalAspectEnum.Stop
                    };
                    UpdateSignal(id, name, aspect);
                    OnClientMessage($"Sim flip: {id} → {aspect}", DccClientOperation.Signal, DccClientMessageType.Inbound);
                    break;
                }
            }
        }

        private void SendBurst() {
            if (Status != DccClientStatus.Connected) return;

            var burstSize = _rand.Next(Math.Max(1, _burstSizeMin), Math.Max(_burstSizeMin + 1, _burstSizeMax + 1));
            var kinds = new[] { "NT", "RT", "SN", "LT", "BK", "SG" };

            for (int i = 0; i < burstSize; i++) {
                var kind = kinds[_rand.Next(kinds.Length)];
                switch (kind) {
                    case"NT": {
                        var (id, name) = Pick("NT", 101, SeedCount, "Turnout");
                        UpdateTurnout(id, name, _rand.Next(2) == 0 ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown);
                        break;
                    }

                    case"RT": {
                        var (id, name) = Pick("RT", 201, SeedCount, "Route");
                        UpdateRoute(id, name, _rand.Next(2) == 0 ? RouteStateEnum.Active : RouteStateEnum.Inactive);
                        break;
                    }

                    case"SN": {
                        var (id, name) = Pick("SN", 301, SeedCount, "Sensor");
                        UpdateSensor(id, name, _rand.Next(2) == 0);
                        break;
                    }

                    case"LT": {
                        var (id, name) = Pick("LT", 401, SeedCount, "Light");
                        UpdateLight(id, name, _rand.Next(2) == 0);
                        break;
                    }

                    case"BK": {
                        var (id, name) = Pick("BK", 501, SeedCount, "Block");
                        UpdateBlock(id, name, _rand.Next(2) == 0);
                        break;
                    }

                    case"SG": {
                        var (id, name) = Pick("SG", 601, SeedCount, "Signal");
                        var aspect = _rand.Next(3) switch {
                            0 => SignalAspectEnum.Clear,
                            1 => SignalAspectEnum.Approach,
                            _ => SignalAspectEnum.Stop
                        };
                        UpdateSignal(id, name, aspect);
                        break;
                    }
                }
            }

            OnClientMessage($"SIM: burst x{burstSize}", DccClientOperation.System, DccClientMessageType.Inbound);
        }

        private static (string id, string name) Pick(string prefix, int start, int count, string label) {
            var n = Random.Shared.Next(count);
            var id = $"{prefix}{start + n}";
            return(id, $"{label} {start + n}");
        }

        private static int GetInt(object obj, string property, int dflt) => obj.GetType().GetProperty(property)?.GetValue(obj) is int i ? i : dflt;

        private static double GetDouble(object obj, string property, double dflt) => obj.GetType().GetProperty(property)?.GetValue(obj) is double x ? x : dflt;

        public void Dispose() => StopTimers();
    }
}