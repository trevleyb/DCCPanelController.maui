using System.Dynamic;
using DccClients.Jmri.Client;
using DccClients.Jmri.EventArgs;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCCommon.Events;

namespace DccClients.Jmri;

public class DccJmriClient : DccClientBase, IDccClient {
    private JmriClientSettings? _settings;
    private bool _isConnected;
    private JmriClient? _jmriClient;

    public DccClientType Type => DccClientType.Jmri;
    
    public DccJmriClient(IDccClientSettings? settings) {
        _settings = settings as Client.JmriClientSettings ?? throw new ArgumentException("Invalid settings provided.");
    }

    public async Task<IResult> ConnectAsync(IDccClientSettings? settings) {
        if (settings is not JmriClientSettings jmriSettings) return Result.Fail("Invalid settings provided.");
        _settings = jmriSettings;
        return await ConnectAsync();
    }

    public async Task<IResult> ConnectAsync() {
        try {
            ArgumentNullException.ThrowIfNull(_settings);
            _jmriClient = new JmriClient(_settings);

            // Subscribe to JMRI client events
            _jmriClient.TurnoutChanged += OnJmriTurnoutChanged;
            _jmriClient.RouteChanged += OnJmriRouteChanged;
            _jmriClient.OccupancyChanged += OnJmriOccupancyChanged;
            _jmriClient.SignalChanged += OnJmriSignalChanged;
            _jmriClient.ConnectionStatusChanged += JmriClientOnConnectionStatusChanged;

            var result = await _jmriClient.Open();
            if (result.IsFailure) {
                OnConnectionStateChanged(new DccStateChangedArgs(false, $"Failed to initialise JMRI client: {result.Message}."));
                return result;
            }

            OnConnectionStateChanged(new DccStateChangedArgs(true, "Connected to JMRI server."));
            _isConnected = true;
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to connect to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> ReconnectAsync() {
        try {
            if (_jmriClient != null) await DisconnectAsync();
            return await ConnectAsync();
        } catch (Exception ex) {
            _isConnected = false;
            return Result.Fail(new Error("Failed to reconnect to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> DisconnectAsync() {
        OnConnectionStateChanged(new DccStateChangedArgs(false, "Disconnected from JMRI server"));
        try {
            if (_jmriClient != null) await _jmriClient.StopAsync();
            _jmriClient = null;
            _isConnected = false;
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to disconnect from JMRI server").CausedBy(ex));
        }
    }

    public bool IsConnected => _isConnected && _jmriClient != null;

    public async Task<IResult> ValidateConnectionAsync() {
        try {
            var result = await ConnectAsync(_settings);
            if (result.IsFailure) return result;
            await Task.Delay(1000);
            if (IsConnected) {
                await DisconnectAsync();
                return Result.Ok("Successfully connected to the server.");
            } else {
                return Result.Fail($"Unable to connect to the server. Check settings.");
            }
        } catch (Exception ex) {
            return Result.Fail(new Error("Unable to connect to the server.").CausedBy(ex));
        }
    }

    public async Task<IResult<IDccClientSettings?>> GetAutomaticConnectionDetailsAsync() {
        var findSettings = _settings ?? new JmriClientSettings();
        
        var findServers = await FindAvailableServicesAsync();
        if (findServers.IsFailure) return Result<IDccClientSettings?>.Fail("No available servers could be found.");
        
        var firstServer = findServers?.Value?.FirstOrDefault();
        if (firstServer is null) return Result<IDccClientSettings?>.Fail("Unable to set connection automatically. No WiThrottle servers found."); 
        
        findSettings.Address = firstServer.Address.ToString();
        findSettings.Port = firstServer.Port;
        return Result<IDccClientSettings?>.Ok(findSettings);
    }
    
    public async Task<IResult<List<DiscoveredService>>> FindAvailableServicesAsync() {
        try {
            var result = await DiscoverServices.SearchForServicesByTypeAsync(DccClientType.Jmri);
            if (result is { IsSuccess: true, Value.Count: > 0 }) return result;
        } catch (Exception ex) {
            return Result<List<DiscoveredService>>.Fail(new Error("Unable to find a WiThrottle server.").CausedBy(ex));
        }
        return Result<List<DiscoveredService>>.Fail("Unable to find a WiThrottle server.");
    }
    
    public async Task<IResult> SendCmdAsync(string message) {
        try {
            if (!IsConnected) return Result.Fail(new Error("Not connected to JMRI server"));
            OnMessageReceived(new DccMessageArgs("Command", message));
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to send command to JMRI server").CausedBy(ex));
        }
        await Task.CompletedTask;
        return Result.Ok();
    }

    public async Task<IResult> SendTurnoutCmdAsync(DccClientCmdProp properties, bool thrown) {
        try {
            if (!IsConnected || _jmriClient == null) return Result.Fail(new Error("Not connected to JMRI server"));
            OnMessageReceived(new DccMessageArgs("Turnout", $"Setting turnout {properties.SystemName} to {(thrown ? "THROWN" : "CLOSED")}"));
            await _jmriClient.SendTurnoutCommandAsync(properties.SystemName, thrown);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to send turnout command to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> SendRouteCmdAsync(DccClientCmdProp properties, bool active) {
        try {
            if (!IsConnected || _jmriClient == null) return Result.Fail(new Error("Not connected to JMRI server"));
            OnMessageReceived(new DccMessageArgs("Route", $"Setting route {properties.SystemName} to {(active ? "ACTIVE" : "INACTIVE")}"));
            if (active) {
                await _jmriClient.SendRouteCommandAsync(properties.SystemName);
            }
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to send route command to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> SendSignalCmdAsync(DccClientCmdProp properties, SignalAspectEnum aspect) {
        try {
            if (!IsConnected || _jmriClient == null) return Result.Fail(new Error("Not connected to JMRI server"));
            OnMessageReceived(new DccMessageArgs("Signal", $"Setting signal {properties.SystemName} to aspect {aspect}"));
            await _jmriClient.SendSignalCommandAsync(properties.SystemName, aspect);
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to send signal command to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> ForceRefreshAsync(string? type = null) {
        return await _jmriClient?.ForceRefreshAsync(type)!;
    }

    private void JmriClientOnConnectionStatusChanged(object? sender, ConnectionStatusEventArgs e) {
        OnConnectionStateChanged(new DccStateChangedArgs(e.IsConnected, e.Message));
        Console.WriteLine($"DccJmriClient: Connection Status Changed: {e.IsConnected} - {e.Message}");
    }

    #region Event Handlers
    private void OnJmriTurnoutChanged(object? sender, TurnoutEventArgs e) {
        var dccAddress = e.DccAddress?.ToString() ?? e.Identifier;
        var isThrown = e.State.Equals("THROWN", StringComparison.OrdinalIgnoreCase);
        OnTurnoutMsgReceived(new DccTurnoutArgs(dccAddress, e.Identifier, isThrown));
    }

    private void OnJmriRouteChanged(object? sender, RouteEventArgs e) {
        var isActive = e.State.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase);
        OnRouteMsgReceived(new DccRouteArgs(e.Identifier, e.Identifier, isActive));
    }

    private void OnJmriOccupancyChanged(object? sender, OccupancyEventArgs e) {
        OnOccupancyMsgReceived(new DccOccupancyArgs(e.TrainId, e.Identifier, e.IsOccupied));
    }

    private void OnJmriSignalChanged(object? sender, SignalEventArgs e) {
        OnSignalMsgReceived(new DccSignalArgs(e.Identifier, e.State));
    }
    #endregion
}