using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients.Helpers;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Clients;

public abstract partial class DccClientBase(Profile profile) : ObservableObject {
    protected const int ReconnectDelayMs = 5000;

    public event EventHandler<DccClientEvent>? ClientMessage;
    [ObservableProperty] private DccClientState _state;

    protected Profile Profile = profile;

    // Helper methods for populating the Profile data with new data that 
    // has been received from a server. 
    // ---------------------------------------------------------------------------
    protected void UpdateTurnout(string id, string name, TurnoutStateEnum state, int? dccAddress = null) {
        if (Profile?.Turnouts is { } collection) {
            Turnout? entity = null;
            entity ??= Profile?.Turnouts?.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile?.Turnouts?.FirstOrDefault(x => x.Name == name) ?? null;
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

    protected void UpdateRoute(string id, string name, RouteStateEnum state, int? dccAddress = null) {
        if (Profile?.Routes is { } collection) {
            Route? entity = null;
            entity ??= Profile?.Routes?.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile?.Routes?.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = state;
            } else {
                entity = new Route {
                    Id = id,
                    Name = name,
                    State = state,
                };
                collection.Add(entity);
            }
        }
    }

    protected void UpdateSensor(string id, string name, bool isOccupied, int? dccAddress = null) {
        if (Profile?.Sensors is { } collection) {
            Sensor? entity = null;
            entity ??= Profile?.Sensors?.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile?.Sensors?.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = isOccupied;
            } else {
                entity = new Sensor {
                    Id = id,
                    Name = name,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    State = isOccupied,
                };
                collection?.Add(entity);
            }
        }
    }

    protected void UpdateBlock(string id, string name, bool isOccupied, string sensor = "", int? dccAddress = null) {
        if (Profile?.Blocks is { } collection) {
            Block? entity = null;
            entity ??= Profile?.Blocks?.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile?.Blocks?.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.IsOccupied = isOccupied;
                entity.Sensor = sensor;
            } else {
                entity = new Block {
                    Id = id,
                    Name = name,
                    IsOccupied = isOccupied,
                    Sensor = sensor,
                };
                collection.Add(entity);
            }
        }
    }

    protected void UpdateLight(string id, string name, bool isOccupied, int? dccAddress = null) {
        if (Profile?.Lights is { } collection) {
            Light? entity = null;
            entity ??= Profile?.Lights?.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile?.Lights?.FirstOrDefault(x => x.Name == name) ?? null;
            if (entity is { }) {
                entity.State = isOccupied;
            } else {
                entity = new Light {
                    Id = id,
                    Name = name,
                    DccAddress = dccAddress ?? id.FromDccAddressString(),
                    IsEditable = false,
                    State = isOccupied,
                };
                collection.Add(entity);
            }
        }
    }

    protected void UpdateSignal(string id, string name, SignalAspectEnum aspect, int? dccAddress = null) {
        if (Profile?.Signals is { } collection) {
            Signal? entity = null;
            entity ??= Profile?.Signals?.FirstOrDefault(x => x.Id == id) ?? null;
            entity ??= Profile?.Signals?.FirstOrDefault(x => x.Name == name) ?? null;
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

    // Helper Methods to raise messages for other consumers to know the 
    // status of the connection and a message if that needs to be added to a queue 
    // or to be logged. 
    // ---------------------------------------------------------------------------
    protected virtual void OnClientMessage(string message, DccClientOperation operation = DccClientOperation.System, DccClientMessageType msgType = DccClientMessageType.System) => OnClientMessage(new DccClientMessage(message, operation, msgType));

    protected virtual void OnClientMessage(DccClientMessage message) => OnClientMessage(new DccClientEvent(State, message));

    protected virtual void OnClientMessage() => ClientMessage?.Invoke(this, new DccClientEvent(State, null));

    protected virtual void OnClientMessage(DccClientEvent e) => ClientMessage?.Invoke(this, e);
}