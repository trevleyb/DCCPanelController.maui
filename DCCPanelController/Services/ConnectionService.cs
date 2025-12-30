using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Models.DataModel.Entities;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Services;

public partial class ConnectionService : ObservableObject {
    private const int MaxServerMessages   = 100;
    private const int ClearServerMessages = 20;

    private int     _initOnceFlag = 0;
    private string? _currentClientKind;

    private readonly ILogger<ConnectionService>    _logger = LogHelper.CreateLogger<ConnectionService>();
    private readonly ProfileService.ProfileService _profileService;
    static string GetClientKind(IDccClientSettings s) => $"{s.GetType().FullName}";

    [ObservableProperty] private DccClientState                         _connectionState;
    [ObservableProperty] private ObservableCollection<DccClientMessage> _serverMessages = [];

    public ConnectionService(ProfileService.ProfileService profileService) {
        _profileService = profileService;
    }

    public IDccClient? Client { get; private set; }
    public ProfileService.ProfileService? ProfileService => _profileService;

    public static ConnectionService Instance => MauiProgram.ServiceHelper.GetService<ConnectionService>();
    public event EventHandler<DccClientState>? ConnectionStateChanged;
    public event EventHandler<DccClientEvent>? ConnectionEvent;

    public async Task InitializeAsync() {
        ConnectionState = DccClientState.Disconnected;
        try {
            if (_profileService.ActiveProfile?.Settings.ConnectOnStartup ?? false) await ConnectAsync();
            _logger.LogInformation("Initialised Connection Service");
        } catch (Exception ex) {
            _logger.LogError("Initialisation of Connection Service error: {Message}", ex.Message);
        }
    }

    public async Task<IResult> ToggleConnectionAsync() {
        switch (ConnectionState) {
        case DccClientState.Initialising:
            _ = await DisconnectAsync();
            return await ConnectAsync();

        case DccClientState.Connected:
            return await DisconnectAsync();

        case DccClientState.Disconnected:
            return await ConnectAsync();

        case DccClientState.Error:
            _ = await DisconnectAsync();
            return await ConnectAsync();

        case DccClientState.Reconnecting:
            _ = await DisconnectAsync();
            return await ConnectAsync();
        }
        throw new Exception("Connection not initialized");
    }

    public async Task<IResult> ConnectAsync() {
        if (_profileService.ActiveProfile?.Settings.ClientSettings is { } settings) {
            return await ConnectAsync(settings);
        }

        return Result.Fail("Unable to connect to the server.");
    }

    public async Task<IResult> ConnectAsync(IDccClientSettings settings) {
        try {
            if (Client is { State: DccClientState.Connected }) await DisconnectAsync();
            if (_profileService.ActiveProfile is { } activeProfile) {
                Client = DccClientFactory.CreateClient(activeProfile, settings);
                Client.ClientMessage += ClientOnClientMessage;
                if (Client is null) {
                    OnConnectionChanged(DccClientState.Error);
                    _logger.LogDebug("Unable to create a Client instance.");
                    return Result.Fail("Unable to create a Client instance.");
                }

                // … unchanged below …
                var newKind = GetClientKind(settings);
                if (!string.Equals(_currentClientKind, newKind, StringComparison.Ordinal)) {
                    Interlocked.Exchange(ref _initOnceFlag, 0);
                    _currentClientKind = newKind;
                }

                // Clear the Source of the items so they will be updated by the connection. 
                await SetAccessorySourceToUnknown();

                var connectResult = await Client.ConnectAsync(); // wraps inner.ConnectAsync()
                if (connectResult.IsFailure) {
                    OnConnectionChanged(DccClientState.Error);
                    _logger.LogDebug("Unable to connect to the specified server.");
                    return Result.Fail("Unable to connect to the specified server.");
                }

                ConnectionStateChanged += OnConnectionStateChanged;
                return connectResult;
            }
        } catch (Exception ex) {
            _logger.LogDebug("Connection Failed: {Message}", ex.Message);
            OnConnectionChanged(DccClientState.Error);
            return Result.Fail(ex, "Unable to connect to the server.");
        }

        _logger.LogDebug("Unable to connect to the specified server.");
        return Result.Fail("Unable to connect to the server.");
    }

    private async void OnConnectionStateChanged(object? sender, DccClientState state) {
        if (state != DccClientState.Connected) return;

        // Run exactly once for this connection kind.
        if (Interlocked.Exchange(ref _initOnceFlag, 1) != 0) return;
        try {
            await SetTurnoutsToDefaultState();
            await ResetOccupancyStates();
        } catch (Exception ex) {
            _logger.LogError(ex, "Post-connect initialization failed.");
        }
    }

    /// <summary>
    /// For any accessories, if the accessory is marked as Manual, then leave it alone. But if it
    /// is marked as another system, then we switch it to UNKNOWN and then we will update it once
    /// we get the data from the connected system. 
    /// </summary>
    private async Task SetAccessorySourceToUnknown() {
        var profile = _profileService.ActiveProfile;
        if (profile is { }) {
            foreach (var accessory in profile.Blocks.Where(b => b.Source != AccessorySource.Manual)) accessory.Source = AccessorySource.Unknown;
            foreach (var accessory in profile.Routes.Where(b => b.Source != AccessorySource.Manual)) accessory.Source = AccessorySource.Unknown;
            foreach (var accessory in profile.Lights.Where(b => b.Source != AccessorySource.Manual)) accessory.Source = AccessorySource.Unknown;
            foreach (var accessory in profile.Sensors.Where(b => b.Source != AccessorySource.Manual)) accessory.Source = AccessorySource.Unknown;
            foreach (var accessory in profile.Signals.Where(b => b.Source != AccessorySource.Manual)) accessory.Source = AccessorySource.Unknown;
            foreach (var accessory in profile.Turnouts.Where(b => b.Source != AccessorySource.Manual)) accessory.Source = AccessorySource.Unknown;
        }
    }

    public async Task<IResult> DisconnectAsync() {
        if (Client is { }) {
            await ResetOccupancyStates();
            if (Client.State == DccClientState.Connected) await Client.DisconnectAsync();
            Client.ClientMessage -= ClientOnClientMessage;
            ConnectionStateChanged -= OnConnectionStateChanged;
            Client = null;
        }
        _profileService.ActiveProfile?.FastClockState = FastClockStateEnum.Off;
        return Result.Ok();
    }

    private async void ClientOnClientMessage(object? sender, DccClientEvent e) {
        try {
            var lastConnectionState = ConnectionState;
            ConnectionState = e.State;

            ConnectionEvent?.Invoke(this, e);
            if (lastConnectionState != ConnectionState) OnConnectionChanged(ConnectionState);
            AddServerMessage(e.Message ?? new DccClientMessage("Unknown Event."));
        } catch (Exception ex) {
            _logger.LogWarning("Client Message Error: " + ex.Message);
        }
    }

    private void OnConnectionChanged(DccClientState state) {
        ConnectionState = state;
        ConnectionStateChanged?.Invoke(this, state);
    }

    // This resets any occupancy states. They will be updated on the next connection
    // -------------------------------------------------------------------------------
    public async Task ResetOccupancyStates() {
        var profile = _profileService.ActiveProfile;
        if (profile is { }) {
            foreach (var sensor in profile.Sensors) sensor.State = false;
            foreach (var block in profile.Blocks) block.IsOccupied = false;
        }
    }

    public async Task SetTurnoutsToDefaultState(bool force = false) {
        var profile = _profileService.ActiveProfile;
        if ( profile is {} && (force || profile?.Settings.SetTurnoutStatesOnStartup == true)) {
            if (Client is { State: DccClientState.Connected }) {
                foreach (var turnout in profile.Turnouts) {
                    if (turnout.Default != TurnoutStateEnum.Unknown && !string.IsNullOrEmpty(turnout.Id)) {
                        await Client.SendTurnoutCmdAsync(turnout, turnout.Default == TurnoutStateEnum.Thrown);
                        await Task.Delay(10);   // A short delay to allow messages to send
                    }
                }
            }
        }
    }

    public virtual void AddServerMessage(string message, DccClientOperation operation = DccClientOperation.System, DccClientMessageType msgType = DccClientMessageType.System) => AddServerMessage(new DccClientMessage(message, operation, msgType));

    public virtual void AddServerMessage(DccClientMessage message) {
        MainThread.BeginInvokeOnMainThread(() => TrimHead(ServerMessages, MaxServerMessages, ClearServerMessages));

        ServerMessages.Add(message);
        #if DEBUG
        _logger.LogDebug($"SERVER: {message.MessageType.ToString()} @ {message.Operation.ToString()} => {message.Message}");
        #endif
        OnPropertyChanged(nameof(ServerMessages));
    }

    [RelayCommand]
    public void ClearMessages() {
        ServerMessages.Clear();
        OnPropertyChanged(nameof(ServerMessages));
    }

    void TrimHead<T>(ObservableCollection<T> items, int max, int clearItems) {
        if (items.Count <= max) return;
        var removeCount = Math.Min(clearItems, items.Count);
        for (int i = 0; i < removeCount; i++) items.RemoveAt(0);
    }
}