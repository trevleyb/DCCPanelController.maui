using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Jmri.View;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.Simulator.View;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Clients.WiThrottle.View;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using Block = DCCPanelController.Models.DataModel.Accessories.Block;
using JmriSettingsViewModel = DCCPanelController.Clients.Jmri.View.JmriSettingsViewModel;
using Light = DCCPanelController.Models.DataModel.Accessories.Light;
using Route = DCCPanelController.Models.DataModel.Accessories.Route;
using SettingsViewModel = DCCPanelController.Clients.Helpers.SettingsViewModel;
using SimulatorSettingsViewModel = DCCPanelController.Clients.Simulator.View.SimulatorSettingsViewModel;
using WiThrottleSettingsViewModel = DCCPanelController.Clients.WiThrottle.View.WiThrottleSettingsViewModel;

namespace DCCPanelController.View;

public partial class DccClientTestViewModel : ObservableObject {
    private readonly ConnectionService _svc;
    private readonly ProfileService    _prf;
    public Profile Profile { get; init; }
    public ObservableCollection<DccClientMessage> ServerMessages => _svc.ServerMessages;
    public event EventHandler<bool>? ConnectStateChanged;

    public DccClientTestViewModel(ProfileService profile, ConnectionService svc) {
        _svc = svc;
        _prf = profile;
        Profile = profile?.ActiveProfile ?? throw new NullReferenceException(nameof(Profile));
        ClientTypes = new ObservableCollection<DccClientType>(new[] {
            DccClientType.Jmri, DccClientType.WiThrottle, DccClientType.Simulator
        });
        
        SelectedClientType = DccClientType.Simulator;
        OnSelectedClientTypeChanged(SelectedClientType);

        // seed pickers
        TurnoutStates = new ObservableCollection<TurnoutStateEnum>(new[] { TurnoutStateEnum.Closed, TurnoutStateEnum.Thrown });
        RouteStates = new ObservableCollection<RouteStateEnum>(new[] { RouteStateEnum.Inactive, RouteStateEnum.Active });
        BinaryStates = new ObservableCollection<string>(new[] { "Off", "On" });
        OccupancyStates = new ObservableCollection<string>(new[] { "Free", "Occupied" });
        SignalAspects = new ObservableCollection<SignalAspectEnum>(new[] { SignalAspectEnum.Clear, SignalAspectEnum.Approach, SignalAspectEnum.Stop, SignalAspectEnum.Off });

        ConnectionStatusText = "Disconnected";
        ConnectionIndicatorColor = Colors.Red;
        ConnectionState = DccClientState.Disconnected;

        RefreshCurrentStates();
        HookMessageStream(); 
    }

    // ---------- Client selection + settings ----------
    public ObservableCollection<DccClientType> ClientTypes { get; }
    [ObservableProperty] private DccClientType _selectedClientType = DccClientType.Simulator;

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

    [ObservableProperty] private string         _connectionStatusText;
    [ObservableProperty] private Color          _connectionIndicatorColor;
    [ObservableProperty] private DccClientState _connectionState;
    [ObservableProperty] private bool           _autoScroll;
    [ObservableProperty] private bool           _isConnectButtonAvailable;
    [ObservableProperty] private bool           _isDisconnectButtonAvailable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotConnected))]
    private bool _isConnected;

    public bool IsNotConnected => !IsConnected;

    //public bool IsConnectionAvailable => IsConnectButtonAvailable;
    public bool IsJmri => SelectedClientType == DccClientType.Jmri;
    public bool IsWiThrottle => SelectedClientType == DccClientType.WiThrottle;
    public bool IsSimulator => SelectedClientType == DccClientType.Simulator;

    partial void OnSelectedClientTypeChanged(DccClientType value) {
        ClearMessages();
        switch (value) {
            case DccClientType.Jmri:
                ActiveSettings = new JmriSettings { Address = "127.0.0.1", Port = 12080, Name = DeviceInfo.Name, SetAutomatically = true };
                JmriSettingsVM = new JmriSettingsViewModel(ActiveSettings, _svc);
                WiThrottleSettingsVM = null;
                SimulatorSettingsVM = null;
                ActiveSettingsVM = JmriSettingsVM;
                SettingsView = new JmriSettingsView(JmriSettingsVM);
            break;

            case DccClientType.WiThrottle:
                ActiveSettings = new WiThrottleSettings { Address = "127.0.0.1", Port = 12090, Name = DeviceInfo.Name, SetAutomatically = true };
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
        OnPropertyChanged(nameof(SettingsView));
        
        IsConnected = false;
        IsConnectButtonAvailable = true;
        IsDisconnectButtonAvailable = false;
    }

    // ---------- Connect / Disconnect ----------
    [RelayCommand]
    private async Task ConnectAsync() {
        if (ActiveSettingsVM is null || ActiveSettings is null) return;
        ClearMessages();
        IsConnected = false;
        ConnectionStatusText = "ATTEMPTING TO CONNECT";
        ConnectionIndicatorColor = Colors.BlueViolet;

        var result = await _svc.ConnectAsync(ActiveSettings);
        if (result.IsFailure) {
            AddMessage($"ERROR: {result.Message}", DccClientOperation.System, DccClientMessageType.Error);
            return;
        }
        AddMessage($"Connected ({SelectedClientType})", DccClientOperation.System, DccClientMessageType.System);
        UnHookMessageStream();
        HookMessageStream();

        RefreshCurrentStates();
        AutoScroll = true;
        ConnectStateChanged?.Invoke(this, true);
    }

    [RelayCommand]
    private async Task DisconnectAsync() {
        await _svc.DisconnectAsync();
        IsConnected = false;
        AddMessage("Disconnected", DccClientOperation.System, DccClientMessageType.System);
        ConnectStateChanged?.Invoke(this, false);
    }

    partial void OnTurnoutChanged(Turnout? value) => RefreshCurrentStates();
    partial void OnBlockChanged(Block? value) => RefreshCurrentStates();
    partial void OnRouteChanged(Route? value) => RefreshCurrentStates();
    partial void OnSignalChanged(Signal? value) => RefreshCurrentStates();
    partial void OnSensorChanged(Sensor? value) => RefreshCurrentStates();
    partial void OnLightChanged(Light? value) => RefreshCurrentStates();

    // ---------- Senders ----------
    private string? TurnoutId => Turnout?.SystemId ?? "";
    [ObservableProperty] private Turnout?         _turnout;
    [ObservableProperty] private TurnoutStateEnum _turnoutDesired      = TurnoutStateEnum.Closed;
    [ObservableProperty] private string           _turnoutCurrentText  = "–";
    [ObservableProperty] private Color            _turnoutCurrentColor = Colors.Gray;

    private string? RouteId => Route?.SystemId ?? "";
    [ObservableProperty] private Route?         _route;
    [ObservableProperty] private RouteStateEnum _routeDesired      = RouteStateEnum.Inactive;
    [ObservableProperty] private string         _routeCurrentText  = "–";
    [ObservableProperty] private Color          _routeCurrentColor = Colors.Gray;

    private string? SensorId => Sensor?.SystemId ?? "";
    [ObservableProperty] private Sensor? _sensor;
    [ObservableProperty] private string  _sensorDesired      = "Off";
    [ObservableProperty] private string  _sensorCurrentText  = "–";
    [ObservableProperty] private Color   _sensorCurrentColor = Colors.Gray;

    private string? LightId => Light?.SystemId ?? "";
    [ObservableProperty] private Light? _light;
    [ObservableProperty] private string _lightDesired      = "Off";
    [ObservableProperty] private string _lightCurrentText  = "–";
    [ObservableProperty] private Color  _lightCurrentColor = Colors.Gray;

    private string? BlockId => Block?.SystemId ?? "";
    [ObservableProperty] private Block? _block;
    [ObservableProperty] private string _blockDesired      = "Free";
    [ObservableProperty] private string _blockCurrentText  = "–";
    [ObservableProperty] private Color  _blockCurrentColor = Colors.Gray;

    private string? SignalId => Signal?.SystemId ?? "";
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
            await c.SendTurnoutCmdAsync(new Turnout { SystemId = TurnoutId, Name = TurnoutId }, TurnoutDesired == TurnoutStateEnum.Thrown);
        }, DccClientOperation.Turnout, $"TX Turnout {TurnoutId} → {TurnoutDesired}");

    [RelayCommand] private async Task SendRouteAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(RouteId)) return;
            await c.SendRouteCmdAsync(new Route { SystemId = RouteId, Name = RouteId }, RouteDesired == RouteStateEnum.Active);
        }, DccClientOperation.Route, $"TX Route {RouteId} → {RouteDesired}");

    [RelayCommand] private async Task SendSensorAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(SensorId)) return;
            await c.SendSensorCmdAsync(new Sensor { SystemId = SensorId, Name = SensorId }, SensorDesired == "On");
        }, DccClientOperation.Sensor, $"TX Sensor {SensorId} → {SensorDesired}");

    [RelayCommand] private async Task SendLightAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(LightId)) return;
            await c.SendLightCmdAsync(new Light { SystemId = LightId, Name = LightId }, LightDesired == "On");
        }, DccClientOperation.Light, $"TX Light {LightId} → {LightDesired}");

    [RelayCommand] private async Task SendBlockAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(BlockId)) return;
            await c.SendBlockCmdAsync(new Block { SystemId = BlockId, Name = BlockId }, BlockDesired == "Occupied");
        }, DccClientOperation.Block, $"TX Block {BlockId} → {BlockDesired}");

    [RelayCommand] private async Task SendSignalAsync() => await SendWrap(async c => {
            if (string.IsNullOrWhiteSpace(SignalId)) return;
            await c.SendSignalCmdAsync(new Signal { SystemId = SignalId, Name = SignalId }, SignalDesired);
        }, DccClientOperation.Signal, $"TX Signal {SignalId} → {SignalDesired}");

    private async Task SendWrap(Func<IDccClient, Task> sender, DccClientOperation operation, string log) {
        var client = GetClientOrNull();
        if (client is null) {
            AddMessage("No client connected", DccClientOperation.System, DccClientMessageType.Error);
            return;
        }

        try {
            await sender(client);
            AddMessage(log, operation, DccClientMessageType.Outbound);
        } catch (Exception ex) {
            AddMessage($"Send failed: {ex.Message}", DccClientOperation.System, DccClientMessageType.Error);
        }

        // After send, re-check current states (the event should also update the UI when it arrives)
        RefreshCurrentStates();
    }

    // ---------- Message log ----------
    [RelayCommand] private void ClearMessages() => _svc.ClearMessages();

    [RelayCommand] private async Task CopyMessages() {
        var text = string.Join(Environment.NewLine, _svc.ServerMessages.Select(m => $"[{m.TimeStamp:HH:mm:ss}] {m.Operation}: {m.Message}"));
        await Clipboard.SetTextAsync(text);
    }

    private void AddMessage(string text, DccClientOperation operation, DccClientMessageType msgType) {
        _svc.AddServerMessage(new DccClientMessage(text, operation, msgType));
    }

    private void AddMessage(string text, DccClientMessageType msgType) {
        _svc.AddServerMessage(new DccClientMessage(text, DccClientOperation.System, msgType));
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
                AddMessage($"CONNECTED", DccClientOperation.System, DccClientMessageType.System);
                ConnectionStatusText = "CONNECTED";
                ConnectionIndicatorColor = Colors.Green;
                IsConnectButtonAvailable = false;
                IsDisconnectButtonAvailable = true;
                IsConnected = true;
            break;

            case DccClientState.Disconnected:
                AddMessage($"DISCONNECTED", DccClientOperation.System, DccClientMessageType.System);
                ConnectionStatusText = "DISCONNECTED";
                ConnectionIndicatorColor = Colors.Gray;
                IsConnectButtonAvailable = true;
                IsDisconnectButtonAvailable = false;
                IsConnected = false;
            break;

            case DccClientState.Error:
                AddMessage($"ERROR", DccClientOperation.System, DccClientMessageType.System);
                ConnectionStatusText = "ERROR";
                ConnectionIndicatorColor = Colors.Red;
                IsConnectButtonAvailable = true;
                IsDisconnectButtonAvailable = false;
                IsConnected = false;
            break;

            case DccClientState.Initialising:
                AddMessage($"INITIALISING", DccClientOperation.System, DccClientMessageType.System);
                ConnectionStatusText = "INITIALISING";
                ConnectionIndicatorColor = Colors.Blue;
                IsConnectButtonAvailable = false;
                IsDisconnectButtonAvailable = true;
                IsConnected = true;
            break;

            case DccClientState.Reconnecting:
                AddMessage($"RECONNECTING", DccClientOperation.System, DccClientMessageType.System);
                ConnectionStatusText = "RECONNECTING";
                ConnectionIndicatorColor = Colors.Yellow;
                IsConnectButtonAvailable = false;
                IsDisconnectButtonAvailable = true;
                IsConnected = true;
            break;
        }

        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(ConnectionIndicatorColor));
        OnPropertyChanged(nameof(IsConnectButtonAvailable));
        OnPropertyChanged(nameof(IsDisconnectButtonAvailable));
    }

    // ---------- State lookup ----------
    private IDccClient? GetClientOrNull() {
        var prop = _svc.GetType().GetProperty("DccClient") ?? _svc.GetType().GetProperty("Client");
        return prop?.GetValue(_svc) as IDccClient;
    }

    private void RefreshCurrentStates() {
        var p = Profile;
        MainThread.BeginInvokeOnMainThread(async () => {
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
        });
    }
}