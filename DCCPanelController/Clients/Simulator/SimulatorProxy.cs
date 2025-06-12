using CommunityToolkit.Mvvm.ComponentModel;
using DCCClient.Simulator;
using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Clients.Simulator;

public partial class SimulatorProxy: DccClientBase, IDccClient {
    public static List<DccClientCapability> Capabilities => [DccClientCapability.Lights, DccClientCapability.Blocks, DccClientCapability.Sensors]; 
    public DccClientType Type => DccClientType.Simulator;
    
    public SimulatorProxy(Profile profile, IDccClientSettings clientSettings) : base(profile) {
        Settings = clientSettings as SimulatorSettings ?? throw new InvalidCastException("Incorrect Settings Type provided for Simulator");
    }

    protected SimulatorClient? _client;
    [ObservableProperty] private SimulatorSettings _settings;
 
    #region Connect and Disconnect Methods
    public async Task<IResult> ConnectAsync() {
        Status = DccClientStatus.Connected;
        AddSimulatedData();
        await Task.CompletedTask;
        return Result.Ok();
    }
    
    public async Task<IResult> DisconnectAsync() {
        Status = DccClientStatus.Disconnected;
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> ForceRefreshAsync(DccClientCapability? capability = null) {
        AddSimulatedData();
        return await Task.FromResult(Result.Ok());
    }

    public async Task<IResult> ValidateConnectionAsync() {
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> SetAutomaticSettingsAsync() {
        await Task.CompletedTask;
        return Result.Ok();
    }

    private void AddSimulatedData() {
        for (var i = 101; i < 110; i++) UpdateTurnout($"NT{i}",$"Turnout {i}", Random.Shared.Next(0, 2) == 0 ? Models.DataModel.Entities.TurnoutStateEnum.Closed : Models.DataModel.Entities.TurnoutStateEnum.Thrown);
        for (var i = 201; i < 210; i++) UpdateRoute  ($"RT{i}",$"Route {i}", Random.Shared.Next(0, 2) == 0 ? Models.DataModel.Entities.RouteStateEnum.Active : Models.DataModel.Entities.RouteStateEnum.Inactive);;
        for (var i = 301; i < 310; i++) UpdateSensor($"SN{i}", $"Sensor {i}", Random.Shared.Next(0, 2) == 0 ? true : false);
        for (var i = 401; i < 410; i++) UpdateLight($"LT{i}", $"Light {i}", Random.Shared.Next(0, 2) == 0 ? true : false);
        for (var i = 501; i < 510; i++) UpdateBlock($"BK{i}", $"Block {i}", Random.Shared.Next(0, 2) == 0 ? true : false);
    }
    #endregion 
    
    #region Sender Methods
    public async Task<IResult> SendTurnoutCmdAsync(Turnout turnout, bool thrown) {
        if (string.IsNullOrEmpty(turnout.Id)) return Result.Fail(new Error("Invalid Turnout Id provided."));
        if (string.IsNullOrEmpty(turnout.Name)) return Result.Fail(new Error("Invalid Turnout Name provided."));
        UpdateTurnout(turnout.Id, turnout.Name, thrown ? Models.DataModel.Entities.TurnoutStateEnum.Thrown : Models.DataModel.Entities.TurnoutStateEnum.Closed);
        return await Task.FromResult(Result.Ok());
    }
    
    public async Task<IResult> SendRouteCmdAsync(Route route, bool active) {
        if (string.IsNullOrEmpty(route.Id)) return Result.Fail(new Error("Invalid Route Id provided."));
        if (string.IsNullOrEmpty(route.Name)) return Result.Fail(new Error("Invalid Route Name provided."));
        UpdateRoute(route.Id, route.Name, active ? Models.DataModel.Entities.RouteStateEnum.Active : Models.DataModel.Entities.RouteStateEnum.Inactive);
        return await Task.FromResult(Result.Ok());

    }
    
    public async Task<IResult> SendSignalCmdAsync(Signal signal, SignalAspectEnum aspect) {
        return await Task.FromResult(Result.Ok());
    }
    
    public async Task<IResult> SendLightCmdAsync(Light light, bool isActive) {
        if (string.IsNullOrEmpty(light.Id)) return Result.Fail(new Error("Invalid Light Id provided."));
        if (string.IsNullOrEmpty(light.Name)) return Result.Fail(new Error("Invalid Light Name provided."));
        UpdateLight(light.Id, light.Name, isActive);
        return await Task.FromResult(Result.Ok());
    }
    
    public async Task<IResult> SendBlockCmdAsync(Block block, bool isOccupied) {
        if (string.IsNullOrEmpty(block.Id)) return Result.Fail(new Error("Invalid Block Id provided."));
        if (string.IsNullOrEmpty(block.Name)) return Result.Fail(new Error("Invalid Block Name provided."));
        UpdateBlock(block.Id, block.Name, isOccupied);
        return await Task.FromResult(Result.Ok());
    }
    
    public async Task<IResult> SendSensorCmdAsync(Sensor sensor, bool isOccupied) {
        if (string.IsNullOrEmpty(sensor.Id)) return Result.Fail(new Error("Invalid Sensor Id provided."));
        if (string.IsNullOrEmpty(sensor.Name)) return Result.Fail(new Error("Invalid Sensor Name provided."));
        UpdateSensor(sensor.Id, sensor.Name, isOccupied);
        return await Task.FromResult(Result.Ok());
    }
    #endregion
    
    #region Discover Methods
    public async Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() {
        try {
            var dummyService = new DiscoveredService {
                InstanceName = "Dummy Service",
                FriendlyName = "Test Service Instance",
                HostName = "localhost",
                Port = 0,
                ServiceType = "simulator",
                Addresses = [],
                TxtRecords = new Dictionary<string, string> {
                    { "description", "Dummy service for testing" },
                    { "version", "1.0" }
                }
            };

            var services = new List<DiscoveredService> { dummyService };
            return await Task.FromResult(Result<List<DiscoveredService>>.Ok(services));
        } catch (Exception ex) {
            return await Task.FromResult(Result<List<DiscoveredService>>.Fail($"Failed to create dummy service: {ex.Message}"));
        }
    }
    
    public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
        return await Task.FromResult((IResult<IDccClientSettings?>)Result.Ok());
    }
    #endregion


}