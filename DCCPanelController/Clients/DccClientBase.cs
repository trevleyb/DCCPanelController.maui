using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Clients;

public abstract partial class DccClientBase(Profile profile) : ObservableObject {
    protected       ILogger Logger          = LogHelper.Logger;
    protected const int     ReconnectDelayMs = 5000;

    public event EventHandler<DccClientEvent>? ClientMessage;
    [ObservableProperty] private DccClientState _state = DccClientState.Disconnected;

    protected Profile Profile = profile;

    protected void UpdateTurnout(string id, string name, TurnoutStateEnum state, int? dccAddress = null) => OnUi(() => UpdateTurnoutCore(id, name, state, dccAddress));
    protected void UpdateRoute(string id, string name, RouteStateEnum state, int? dccAddress = null) => OnUi(() => UpdateRouteCore(id, name, state, dccAddress));
    protected void UpdateSensor(string id, string name, bool isOccupied, int? dccAddress = null) => OnUi(() => UpdateSensorCore(id, name, isOccupied, dccAddress));
    protected void UpdateBlock(string id, string name, bool isOccupied, string? sensor = null, int? dccAddress = null) => OnUi(() => UpdateBlockCore(id, name, isOccupied, sensor, dccAddress));
    protected void UpdateLight(string id, string name, bool isOn, int? dccAddress = null) => OnUi(() => UpdateLightCore(id, name, isOn, dccAddress));
    protected void UpdateSignal(string id, string name, SignalAspectEnum aspect, int? dccAddress = null) => OnUi(() => UpdateSignalCore(id, name, aspect, dccAddress));
    protected void UpdateFastClock(DateTime? fastClock, FastClockStateEnum state) => OnUi(() => UpdateFastClockCore(fastClock, state));
    protected void UpdatePowerState(PowerStateEnum state) => OnUi(() => UpdatePowerStateCore(state));
    
    // Helper methods for populating the Profile data with new data that 
    // has been received from a server. 
    // ---------------------------------------------------------------------------
    private void UpdateTurnoutCore(string id, string name, TurnoutStateEnum state, int? dccAddress = null) {
        if (Profile.Turnouts is { } collection) {
            Turnout? entity = null;
            entity ??= Profile.Turnouts.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile.Turnouts.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = state;
            } else {
                entity = new Turnout {
                    Id = id,
                    Name = name,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    IsEditable = false,
                    State = state,
                };
                collection.Add(entity);
            }
        }
    }

    private void UpdateRouteCore(string id, string name, RouteStateEnum state, int? dccAddress = null) {
        if (Profile.Routes is { } collection) {
            Route? entity = null;
            entity ??= Profile.Routes.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile.Routes.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = state;
            } else {
                entity = new Route {
                    Id = id,
                    Name = name,
                    State = state,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                };
                collection.Add(entity);
            }
        }
    }

    private void UpdateSensorCore(string id, string name, bool isOccupied, int? dccAddress = null) {
        if (Profile.Sensors is { } collection) {
            Sensor? entity = null;
            entity ??= Profile.Sensors.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile.Sensors.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = isOccupied;
            } else {
                entity = new Sensor {
                    Id = id,
                    Name = name,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    State = isOccupied,
                };
                collection.Add(entity);
            }
        }
    }

    private void UpdateBlockCore(string id, string name, bool isOccupied, string? sensor, int? dccAddress = null) {
        if (Profile.Blocks is { } collection) {
            Block? entity = null;
            entity ??= Profile.Blocks.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile.Blocks.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.IsOccupied = isOccupied;
                if (sensor is {}) entity.Sensor = sensor;
            } else {
                entity = new Block {
                    Id = id,
                    Name = name,
                    IsOccupied = isOccupied,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    Sensor = sensor ?? "",
                };
                collection.Add(entity);
            }
        }
    }

    private void UpdateLightCore(string id, string name, bool isOn, int? dccAddress = null) {
        if (Profile.Lights is { } collection) {
            Light? entity = null;
            entity ??= Profile.Lights.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile.Lights.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = isOn;
            } else {
                entity = new Light {
                    Id = id,
                    Name = name,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    IsEditable = false,
                    State = isOn,
                };
                collection.Add(entity);
            }
        }
    }

    private void UpdateSignalCore(string id, string name, SignalAspectEnum aspect, int? dccAddress = null) {
        if (Profile.Signals is { } collection) {
            Signal? entity = null;
            entity ??= Profile.Signals.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile.Signals.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.Aspect = aspect.ToString();
            } else {
                entity = new Signal {
                    Id = id,
                    Name = name,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    IsEditable = false,
                    Aspect = aspect.ToString(),
                };
                collection.Add(entity);
            }
        }
    }

    private void UpdateFastClockCore(DateTime? fastClock, FastClockStateEnum state) {
        if (fastClock is { } clock) {
            Profile.FastClockState = state;
            Profile.FastClock = clock;
        }
    }

    private void UpdatePowerStateCore(PowerStateEnum state) {
        Profile.PowerState = state;
    }

    protected static void OnUi(Action action) {
        if (MainThread.IsMainThread) action();
        else MainThread.BeginInvokeOnMainThread(action);
    }

    // Support for Retries
    // ----------------------------------------------------------------------------
    protected async Task RunReconnectLoopAsync(Func<CancellationToken, Task> connectOnce,
        Func<bool> isDisposed,
        int maxRetries,
        TimeSpan initialDelay,
        double multiplier,
        CancellationToken ct) {
        
        var attempt = 0;
        while (!ct.IsCancellationRequested && !isDisposed()) {
            try {
                await connectOnce(ct); 
                if (!ct.IsCancellationRequested) {
                    State = DccClientState.Disconnected;
                    OnClientMessage("Connection closed");
                }
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) {
                State = DccClientState.Error;
                OnClientMessage($"Connection error: {ex.Message}", DccClientOperation.System, DccClientMessageType.Error);
            }

            attempt++;
            State = DccClientState.Reconnecting;
            var delayMs = (int)Math.Min(initialDelay.TotalMilliseconds * Math.Pow(multiplier, attempt - 1), 30_000);
            OnClientMessage($"Reconnecting attempt {attempt} in {delayMs}ms...");
            try {
                await Task.Delay(delayMs, ct);
            } catch {
                break;
            }

            // stop after maxRetries (unless maxRetries <= 0 which means "forever")
            if (maxRetries > 0 && attempt >= maxRetries) {
                State = DccClientState.Disconnected;
                OnClientMessage("Reconnect attempts exhausted");
                break;
            }
        }
    }

    // Helper Methods to raise messages for other consumers to know the 
    // status of the connection and a message if that needs to be added to a queue 
    // or to be logged. 
    // ---------------------------------------------------------------------------
    protected virtual void OnClientMessage(string message, DccClientOperation operation = DccClientOperation.System, DccClientMessageType msgType = DccClientMessageType.System) => OnClientMessage(new DccClientMessage(message, operation, msgType));

    protected virtual void OnClientMessage(DccClientMessage message) => OnClientMessage(new DccClientEvent(State, message));

    protected virtual void OnClientMessage() {
        OnUi(() => ClientMessage?.Invoke(this, new DccClientEvent(State, null)));
    }

    protected virtual void OnClientMessage(DccClientEvent e) {
        OnUi(() => ClientMessage?.Invoke(this, e));
    }
}