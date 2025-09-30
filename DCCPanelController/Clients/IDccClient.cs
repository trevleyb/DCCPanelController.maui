using DCCPanelController.Clients.Discovery;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Clients;

public interface IDccClient {
    DccClientType Type { get; }
    DccClientStatus Status { get; }

    bool IsConnected { get; }
    bool IsDisconnected => !IsConnected;

    public event EventHandler<DccClientEvent>? ClientMessage;

    Task<IResult> ConnectAsync();
    Task<IResult> DisconnectAsync();
    Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null);
    Task<IResult> ValidateConnectionAsync();
    Task<IResult> SetAutomaticSettingsAsync();

    Task<IResult> SendTurnoutCmdAsync(Turnout turnout, bool thrown);
    Task<IResult> SendRouteCmdAsync(Route route, bool active);
    Task<IResult> SendSignalCmdAsync(Signal signal, SignalAspectEnum aspect);
    Task<IResult> SendLightCmdAsync(Light light, bool isActive);
    Task<IResult> SendBlockCmdAsync(Block block, bool isOccupied);
    Task<IResult> SendSensorCmdAsync(Sensor sensor, bool isOccupied);

    Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync();
    Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync();
}