using System.Dynamic;
using DccClients.Jmri.Client;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;
using DCCCommon.Client;
using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCCommon.Events;
using DCCCommon.Helpers;

namespace DccClients.Jmri;

public class DccJmriClient : DccClientBase, IDccClient {
    private JmriClient? _jmriClient;
    private JmriClientSettings? _settings;
    private const int ReconnectDelay = 5000;
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
            _jmriClient?.Dispose();
            _jmriClient = new JmriClient(_settings.Address, _settings.Port, ReconnectDelay);

            // Subscribe to JMRI client events so we can translate them to
            // common events used across the system
            _jmriClient.TurnoutChanged  += OnJmriTurnoutChanged;
            _jmriClient.RouteChanged    += OnJmriRouteChanged;
            _jmriClient.SignalChanged   += OnJmriSignalChanged;
            _jmriClient.BlockChanged    += OnJmriOccupancyChanged;
            _jmriClient.ConnectionStateChanged += OnJmriConnectionStatusChanged;
            _jmriClient.ConnectionEstablished += OnJmriConnectionEstablished;
            
            await _jmriClient.ConnectAsync();

            // Wait for a connection to initialize with timeout
            const int timeoutMs = 10000; // 10 seconds timeout
            const int delayMs = 100;     // 100ms delay between checks
            var elapsed = 0;

            while (elapsed < timeoutMs) {
                Console.WriteLine($"Waiting for Connection: {_jmriClient.ConnectionState}");
                var state = _jmriClient.ConnectionState;
                if (state == ConnectionStateEnum.Connected || state == ConnectionStateEnum.Disconnected) break;
                if (state != ConnectionStateEnum.Initialising) break;
                await Task.Delay(delayMs);
                elapsed += delayMs;
            }
            Console.WriteLine($"Finished waiting: {_jmriClient.ConnectionState}");

            // Check final state after timeout or successful initialization
            var finalState = _jmriClient.ConnectionState;
            if (finalState == ConnectionStateEnum.Initialising) {
                OnConnectionStateChanged(new DccStateChangedArgs(false, "JMRI client initialization timeout"));
                return Result.Fail("Connection initialization timed out.");
            }

            if (finalState != ConnectionStateEnum.Connected) {
                OnConnectionStateChanged(new DccStateChangedArgs(false, $"Failed to connect to JMRI client. State: {finalState}"));
                return Result.Fail("Unable to connect.");
            }

            // Successfully connected
            OnConnectionStateChanged(new DccStateChangedArgs(true, "Connected to JMRI server."));
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
            return Result.Fail(new Error("Failed to reconnect to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> DisconnectAsync() {
        OnConnectionStateChanged(new DccStateChangedArgs(false, "Disconnected from JMRI server"));
        try {
            if (_jmriClient != null) await _jmriClient.DisconnectAsync();
            _jmriClient = null;
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to disconnect from JMRI server").CausedBy(ex));
        }
    }

    public bool IsConnected => _jmriClient?.ConnectionState == ConnectionStateEnum.Connected;

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
            await _jmriClient.SetTurnoutStateAsync(properties.SystemName, thrown);
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
                await _jmriClient.SetRouteStateAsync(properties.SystemName, active);
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
            await _jmriClient.SetSignalAppearanceAsync(properties.SystemName, aspect.ToString());
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to send signal command to JMRI server").CausedBy(ex));
        }
    }

    public async Task<IResult> ForceRefreshAsync(string? type = null) {
        if (_jmriClient is { } client) await client.DisconnectAsync();
        var result = await ConnectAsync();
        return result;
    }

    #region Event Handlers
    private void OnJmriConnectionEstablished(object? sender, JmriInitialisedEventArgs e) {
        var message = $"JMRI Connection established. Version={e.JmriVersion} Railroad={e.Railroad}";
        OnMessageReceived(new DccMessageArgs("Initialised", message));
    }
    
    private void OnJmriConnectionStatusChanged(object? sender, JmriConnectionChangedEventArgs e) {
        Console.WriteLine($"JMRI Connect State Changed: Connected={e.ConnectionState} Message={e.Message} Called={e.CallerDetails}");
        OnConnectionStateChanged(new DccStateChangedArgs(e.IsConnected, e.Message));
    }
    
    private void OnJmriTurnoutChanged(object? sender, JmriTurnoutEventArgs e) {
        OnTurnoutMsgReceived(new DccTurnoutArgs(e.UserName, e.Name, e.State == 4));
    }

    private void OnJmriRouteChanged(object? sender, JmriRouteEventArgs e) {
        OnRouteMsgReceived(new DccRouteArgs(e.UserName, e.Name, e.State == 2));
    }

    private void OnJmriOccupancyChanged(object? sender, JmriBlockEventArgs e) {
        OnOccupancyMsgReceived(new DccOccupancyArgs(e.UserName, e.Name, e.State == 2));
    }

    private void OnJmriSignalChanged(object? sender, JmriSignalEventArgs e) {
        OnSignalMsgReceived(new DccSignalArgs(e.UserName, e.Appearance));
    }
    #endregion
}