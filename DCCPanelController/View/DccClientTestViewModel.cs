using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Settings;
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.Simulator;
using DCCPanelController.View.Settings.WiThrottle;

namespace DCCPanelController.View;

public partial class DccClientTestViewModel : ObservableObject {
    private readonly ConnectionService _svc;
    private readonly ProfileService    _prf;
    public Profile Profile { get; init; }

    public DccClientTestViewModel(ProfileService profile, ConnectionService svc) {
        _svc = svc;
        _prf = profile;
        Profile = profile?.ActiveProfile ?? throw new NullReferenceException(nameof(Profile));
        ClientTypes = new ObservableCollection<DccClientType>(new[] {
            DccClientType.Jmri, DccClientType.WiThrottle, DccClientType.Simulator
        });

        // default: Simulator to be safe
        SelectedClientType = DccClientType.Simulator;

        // seed pickers
        TurnoutStates = new ObservableCollection<TurnoutStateEnum>(new[] { TurnoutStateEnum.Closed, TurnoutStateEnum.Thrown });
        RouteStates = new ObservableCollection<RouteStateEnum>(new[] { RouteStateEnum.Inactive, RouteStateEnum.Active });
        BinaryStates = new ObservableCollection<string>(new[] { "Off", "On" });
        OccupancyStates = new ObservableCollection<string>(new[] { "Free", "Occupied" });
        SignalAspects = new ObservableCollection<SignalAspectEnum>(new[] { SignalAspectEnum.Clear, SignalAspectEnum.Approach, SignalAspectEnum.Stop, SignalAspectEnum.Off });

        ConnectionStatusText = "Disconnected";
        ConnectionIndicatorColor = Colors.Red;
        ConnectionState = DccClientState.Disconnected;
        
        Messages = new ObservableCollection<MsgLine>();
        RefreshCurrentStates();
        HookMessageStream(); // see method body — designed to work with your existing patterns
    }

    // ---------- Client selection + settings ----------
    public ObservableCollection<DccClientType> ClientTypes { get; }
    [ObservableProperty] private DccClientType _selectedClientType;
    [ObservableProperty] private bool          _isConnected;
    [ObservableProperty] private bool          _isNotConnected;

    // Inline settings view models (yours)
    [ObservableProperty] private ContentView?                 _settingsView;
    [ObservableProperty] private SettingsViewModel?           _activeSettingsVM;
    [ObservableProperty] private JmriSettingsViewModel?       _jmriSettingsVM;
    [ObservableProperty] private WiThrottleSettingsViewModel? _wiThrottleSettingsVM;
    [ObservableProperty] private SimulatorSettingsViewModel?  _simulatorSettingsVM;
    [ObservableProperty] private IDccClientSettings?          _activeSettings;

    [ObservableProperty] private bool _supportsTurnouts;
    [ObservableProperty] private bool _supportsBlocks;
    [ObservableProperty] private bool _supportsLights;
    [ObservableProperty] private bool _supportsSensors;
    [ObservableProperty] private bool _supportsSignals;
    [ObservableProperty] private bool _supportsRoutes;

    [ObservableProperty] private string _connectionStatusText;
    [ObservableProperty] private Color  _connectionIndicatorColor;
    [ObservableProperty] private DccClientState _connectionState;

    public bool IsConnectionAvailable => CanConnect;
    public bool IsJmri => SelectedClientType == DccClientType.Jmri;
    public bool IsWiThrottle => SelectedClientType == DccClientType.WiThrottle;
    public bool IsSimulator => SelectedClientType == DccClientType.Simulator;

    public bool CanConnect => ActiveSettingsVM is { } && !IsConnected;
    public bool CanDisconnect => ActiveSettingsVM is { } && IsConnected;


    partial void OnSelectedClientTypeChanged(DccClientType value) {
        // Build a fresh settings VM of the right kind (reusing your pattern)
        switch (value) {
            case DccClientType.Jmri:
                ActiveSettings = new JmriSettings { Address = "127.0.0.1", Port = 12080, Name = DeviceInfo.Name };
                JmriSettingsVM = new JmriSettingsViewModel(ActiveSettings, _svc);
                WiThrottleSettingsVM = null;
                SimulatorSettingsVM = null;
                ActiveSettingsVM = JmriSettingsVM;
                SettingsView = new JmriSettingsView(JmriSettingsVM);
            break;

            case DccClientType.WiThrottle:
                ActiveSettings = new WiThrottleSettings { Address = "127.0.0.1", Port = 12090, Name = DeviceInfo.Name };
                WiThrottleSettingsVM = new WiThrottleSettingsViewModel(ActiveSettings, _svc);
                JmriSettingsVM = null;
                SimulatorSettingsVM = null;
                ActiveSettingsVM = WiThrottleSettingsVM;
                SettingsView = new WiThrottleSettingsView(WiThrottleSettingsVM);
            break;

            default:
                ActiveSettings = new SimulatorSettings { /* TickMs = 1000 */ };
                SimulatorSettingsVM = new SimulatorSettingsViewModel(ActiveSettings, _svc);
                JmriSettingsVM = null;
                WiThrottleSettingsVM = null;
                ActiveSettingsVM = SimulatorSettingsVM;
                SettingsView = new SimulatorSettingsView(SimulatorSettingsVM);
            break;
        }

        SupportsTurnouts = ActiveSettings.Capabilities.Contains(DccClientCapability.Turnouts);
        SupportsBlocks = ActiveSettings.Capabilities.Contains(DccClientCapability.Blocks);
        SupportsLights = ActiveSettings.Capabilities.Contains(DccClientCapability.Lights);
        SupportsSignals = ActiveSettings.Capabilities.Contains(DccClientCapability.Signals);
        SupportsRoutes = ActiveSettings.Capabilities.Contains(DccClientCapability.Routes);
        SupportsSensors = false;

        OnPropertyChanged(nameof(IsJmri));
        OnPropertyChanged(nameof(IsWiThrottle));
        OnPropertyChanged(nameof(IsSimulator));
        OnPropertyChanged(nameof(CanConnect));
    }

    // ---------- Connect / Disconnect ----------
    [RelayCommand]
    private async Task ConnectAsync() {
        if (ActiveSettingsVM is null || ActiveSettings is null) return;
        var result = await _svc.ConnectAsync(ActiveSettings);
        if (result.IsFailure) {
            AddMessage($"ERROR: {result.Message}", "ERR");
            return;
        }
        AddMessage($"Connected ({SelectedClientType})", "SYS");
        UnHookMessageStream();
        HookMessageStream();
        RefreshCurrentStates();
    }

    [RelayCommand]
    private async Task DisconnectAsync() {
        await _svc.DisconnectAsync();
        AddMessage("Disconnected", "SYS");
    }

    partial void OnTurnoutChanged(Turnout? value) => RefreshCurrentStates();
    partial void OnBlockChanged(Block? value) => RefreshCurrentStates();
    partial void OnRouteChanged(Route? value) => RefreshCurrentStates();
    partial void OnSignalChanged(Signal? value) => RefreshCurrentStates();
    partial void OnSensorChanged(Sensor? value) => RefreshCurrentStates();
    partial void OnLightChanged(Light? value) => RefreshCurrentStates();

    // ---------- Senders ----------
    private string? TurnoutId => Turnout?.Id ?? "";
    [ObservableProperty] private Turnout?         _turnout;
    [ObservableProperty] private TurnoutStateEnum _turnoutDesired      = TurnoutStateEnum.Closed;
    [ObservableProperty] private string           _turnoutCurrentText  = "–";
    [ObservableProperty] private Color            _turnoutCurrentColor = Colors.Gray;

    private string? RouteId => Route?.Id ?? "";
    [ObservableProperty] private Route?         _route;
    [ObservableProperty] private RouteStateEnum _routeDesired      = RouteStateEnum.Inactive;
    [ObservableProperty] private string         _routeCurrentText  = "–";
    [ObservableProperty] private Color          _routeCurrentColor = Colors.Gray;

    private string? SensorId => Sensor?.Id ?? "";
    [ObservableProperty] private Sensor? _sensor;
    [ObservableProperty] private string  _sensorDesired      = "Off";
    [ObservableProperty] private string  _sensorCurrentText  = "–";
    [ObservableProperty] private Color   _sensorCurrentColor = Colors.Gray;

    private string? LightId => Light?.Id ?? "";
    [ObservableProperty] private Light? _light;
    [ObservableProperty] private string _lightDesired      = "Off";
    [ObservableProperty] private string _lightCurrentText  = "–";
    [ObservableProperty] private Color  _lightCurrentColor = Colors.Gray;

    private string? BlockId => Block?.Id ?? "";
    [ObservableProperty] private Block? _block;
    [ObservableProperty] private string _blockDesired      = "Free";
    [ObservableProperty] private string _blockCurrentText  = "–";
    [ObservableProperty] private Color  _blockCurrentColor = Colors.Gray;

    private string? SignalId => Signal?.Id ?? "";
    [ObservableProperty] private Signal?          _signal;
    [ObservableProperty] private SignalAspectEnum _signalDesired      = SignalAspectEnum.Clear;
    [ObservableProperty] private string           _signalCurrentText  = "–";
    [ObservableProperty] private Color            _signalCurrentColor = Colors.Gray;

    public ObservableCollection<TurnoutStateEnum> TurnoutStates { get; }
    public ObservableCollection<RouteStateEnum> RouteStates { get; }
    public ObservableCollection<string> BinaryStates { get; }
    public ObservableCollection<string> OccupancyStates { get; }
    public ObservableCollection<SignalAspectEnum> SignalAspects { get; }

    [RelayCommand] private async Task SendTurnoutAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(TurnoutId)) return;
            await c.SendTurnoutCmdAsync(new Turnout { Id = TurnoutId, Name = TurnoutId }, TurnoutDesired == TurnoutStateEnum.Thrown);
        }, $"TX Turnout {TurnoutId} → {TurnoutDesired}");

    [RelayCommand] private async Task SendRouteAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(RouteId)) return;
            await c.SendRouteCmdAsync(new Route { Id = RouteId, Name = RouteId }, RouteDesired == RouteStateEnum.Active);
        }, $"TX Route {RouteId} → {RouteDesired}");

    [RelayCommand] private async Task SendSensorAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(SensorId)) return;
            await c.SendSensorCmdAsync(new Sensor { Id = SensorId, Name = SensorId }, SensorDesired == "On");
        }, $"TX Sensor {SensorId} → {SensorDesired}");

    [RelayCommand] private async Task SendLightAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(LightId)) return;
            await c.SendLightCmdAsync(new Light { Id = LightId, Name = LightId }, LightDesired == "On");
        }, $"TX Light {LightId} → {LightDesired}");

    [RelayCommand] private async Task SendBlockAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(BlockId)) return;
            await c.SendBlockCmdAsync(new Block { Id = BlockId, Name = BlockId }, BlockDesired == "Occupied");
        }, $"TX Block {BlockId} → {BlockDesired}");

    [RelayCommand] private async Task SendSignalAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(SignalId)) return;
            await c.SendSignalCmdAsync(new Signal { Id = SignalId, Name = SignalId }, SignalDesired);
        }, $"TX Signal {SignalId} → {SignalDesired}");

    private async Task SendWrap(Func<IDccClient, Task> sender, string log) {
        var client = GetClientOrNull();
        if (client is null) {
            AddMessage("No client connected", "ERR");
            return;
        }

        try {
            await sender(client);
            AddMessage(log, "OUT");
        } catch (Exception ex) {
            AddMessage($"Send failed: {ex.Message}", "ERR");
        }

        // After send, re-check current states (the event should also update the UI when it arrives)
        RefreshCurrentStates();
    }

    // ---------- Message log ----------
    public ObservableCollection<MsgLine> Messages { get; }
    [RelayCommand] private void ClearMessages() => Messages.Clear();

    [RelayCommand] private async Task CopyMessages() {
        var text = string.Join(Environment.NewLine, Messages.Select(m => $"[{m.Time:HH:mm:ss}] {m.Kind}: {m.Text}"));
        await Clipboard.SetTextAsync(text);
    }

    private void AddMessage(string text, DccClientMessageType msg) {
        var kind = msg switch {
            DccClientMessageType.System   => "SYS",
            DccClientMessageType.Error    => "ERR",
            DccClientMessageType.Inbound  => "IN",
            DccClientMessageType.Outbound => "OUT",
            _                             => "SYS"
        };
        Messages.Insert(0, new MsgLine(text, kind));
        _svc.AddServerMessage(text); // also push into your global log, consistent with SettingsViewModel :contentReference[oaicite:5]{index=5}
    }

    private void AddMessage(string text, string kind) {
        Messages.Insert(0, new MsgLine(text, kind));
        _svc.AddServerMessage(text); // also push into your global log, consistent with SettingsViewModel :contentReference[oaicite:5]{index=5}
    }

    private void HookMessageStream() {
        _svc.ConnectionStateChanged += SvcOnConnectionStateChanged;
        _svc.ConnectionEvent += SvcOnConnectionEvent;
    }

    private void UnHookMessageStream() {
        _svc.ConnectionStateChanged -= SvcOnConnectionStateChanged;
        _svc.ConnectionEvent -= SvcOnConnectionEvent;
    }

    private void SvcOnConnectionEvent(object? sender, DccClientEvent e) {
        var msg = e.Message;
        AddMessage($"{(msg?.Operation.ToString() ?? "Unknown Op")} => {msg?.Message ?? "Unknown Message"}", msg?.MessageType ?? DccClientMessageType.Error);
        RefreshCurrentStates();
    }

    private void SvcOnConnectionStateChanged(object? sender, DccClientState e) {
        ConnectionState = e;
        switch (e) {
            case DccClientState.Connected:
                AddMessage($"***Connection State Change => CONNECTED", "SYS");
                ConnectionStatusText = "Connected";
                ConnectionIndicatorColor = Colors.Green;
                IsConnected = true;
                IsNotConnected = false;
            break;

            case DccClientState.Disconnected:
                AddMessage($"***Connection State Change => DISCONNECTED", "SYS");
                ConnectionStatusText = "Disconnected";
                ConnectionIndicatorColor = Colors.Gray;
                IsConnected = false;
                IsNotConnected = true;
            break;

            case DccClientState.Error:
                AddMessage($"***Connection State Change => ERROR", "SYS");
                ConnectionStatusText = "Error";
                ConnectionIndicatorColor = Colors.Red;
                IsConnected = false;
                IsNotConnected = true;
            break;

            case DccClientState.Initialising:
                AddMessage($"***Connection State Change => INITIALISING", "SYS");
                ConnectionStatusText = "Initialising";
                ConnectionIndicatorColor = Colors.Blue;
                IsConnected = false;
                IsNotConnected = false;
            break;

            case DccClientState.Reconnecting:
                AddMessage($"***Connection State Change => RECONNECTING", "SYS");
                ConnectionStatusText = "Reconnecting";
                ConnectionIndicatorColor = Colors.Yellow;
                IsConnected = false;
                IsNotConnected = false;
            break;
        }

        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(ConnectionIndicatorColor));
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsNotConnected));
        OnPropertyChanged(nameof(CanConnect));
        OnPropertyChanged(nameof(CanDisconnect));
    }

    // ---------- State lookup ----------
    private IDccClient? GetClientOrNull() {
        // Adjust if your service exposes a different property name for the live client
        var prop = _svc.GetType().GetProperty("DccClient") ?? _svc.GetType().GetProperty("Client");
        return prop?.GetValue(_svc) as IDccClient;
    }

    private void RefreshCurrentStates() {
        var p = Profile;

        // Turnout
        if (!string.IsNullOrWhiteSpace(TurnoutId) && p.Turnouts.TryGet(TurnoutId, out var t)) {
            TurnoutCurrentText = t?.State.ToString() ?? "";
            TurnoutCurrentColor = t?.State == TurnoutStateEnum.Thrown ? Colors.OrangeRed : Colors.ForestGreen;
        } else {
            TurnoutCurrentText = "–";
            TurnoutCurrentColor = Colors.Gray;
        }

        // Route
        if (!string.IsNullOrWhiteSpace(RouteId) && p.Routes.TryGet(RouteId, out var r)) {
            RouteCurrentText = r?.State.ToString() ?? "";
            RouteCurrentColor = r?.State == RouteStateEnum.Active ? Colors.ForestGreen : Colors.Gray;
        } else {
            RouteCurrentText = "–";
            RouteCurrentColor = Colors.Gray;
        }

        // Sensor
        if (!string.IsNullOrWhiteSpace(SensorId) && p.Sensors.TryGet(SensorId, out var s)) {
            var on = s?.State ?? false;
            SensorCurrentText = on ? "On" : "Off";
            SensorCurrentColor = on ? Colors.ForestGreen : Colors.Gray;
        } else {
            SensorCurrentText = "–";
            SensorCurrentColor = Colors.Gray;
        }

        // Light
        if (!string.IsNullOrWhiteSpace(LightId) && p.Lights.TryGet(LightId, out var l)) {
            var on = l?.State ?? false;
            LightCurrentText = on ? "On" : "Off";
            LightCurrentColor = on ? Colors.Gold : Colors.Gray;
        } else {
            LightCurrentText = "–";
            LightCurrentColor = Colors.Gray;
        }

        // Block
        if (!string.IsNullOrWhiteSpace(BlockId) && p.Blocks.TryGet(BlockId, out var b)) {
            var occ = b?.IsOccupied ?? false;
            BlockCurrentText = occ ? "Occupied" : "Free";
            BlockCurrentColor = occ ? Colors.OrangeRed : Colors.ForestGreen;
        } else {
            BlockCurrentText = "–";
            BlockCurrentColor = Colors.Gray;
        }

        // Signal
        if (!string.IsNullOrWhiteSpace(SignalId) && p.Signals.TryGet(SignalId, out var sg)) {
            SignalCurrentText = sg?.Aspect.ToString() ?? "Clear";
            SignalCurrentColor = sg?.Aspect switch {
                "Clear"    => Colors.ForestGreen,
                "Approach" => Colors.Goldenrod,
                "Stop"     => Colors.OrangeRed,
                _          => Colors.Gray
            };
        } else {
            SignalCurrentText = "–";
            SignalCurrentColor = Colors.Gray;
        }
    }
}

public record MsgLine(string Text, string Kind) {
    public DateTime Time { get; } = DateTime.Now;

    public Color KindColor =>
        Kind switch {
            "OUT" => Colors.SteelBlue,
            "IN"  => Colors.DarkGreen,
            "ERR" => Colors.OrangeRed,
            "SYS" => Colors.Gray,
            _     => Colors.Gray
        };
}