using DCCClients.Common;
using DCCClients.Events;
using DCCClients.Interfaces;
using DCCClients.JMRI;
using DCCClients.JMRI.EventArgs;

namespace DCCClients;

public class DccJmriClient : DccClient, IDccClient {
    private readonly JmriSettings? _settings;
    private JmriClient? _jmriClient;
    private bool _isConnected;

    public DccJmriClient(IDccSettings? settings) {
        _settings = settings as JmriSettings ?? throw new ArgumentException("Invalid settings provided.");
    }

    /// <summary>
    ///     Establishes a connection to the JMRI server using the provided settings.
    /// </summary>
    /// <returns>Returns a result indicating the success or failure of the connection attempt.</returns>
    public async Task<IResult> ConnectAsync() {
        try {
            ArgumentNullException.ThrowIfNull(_settings);
            
            _jmriClient = CreateJmriClient(_settings);
            
            // Subscribe to JMRI client events
            _jmriClient.TurnoutChanged += OnJmriTurnoutChanged;
            _jmriClient.RouteChanged += OnJmriRouteChanged;
            _jmriClient.OccupancyChanged += OnJmriOccupancyChanged;
            _jmriClient.SignalChanged += OnJmriSignalChanged;
            
            // Initialize the client and fetch initial data
            await _jmriClient.InitializeAsync();
            
            // Start monitoring for changes
            await _jmriClient.StartMonitoringAsync();
            
            _isConnected = true;
            return Result.Ok();
        }
        catch (Exception ex) {
            return Result.Fail(new Error("Failed to connect to JMRI server").CausedBy(ex));
        }
    }

    /// <summary>
    /// Creates a new JmriClient instance. Can be overridden in tests to provide a mock.
    /// </summary>
    protected virtual JmriClient CreateJmriClient(JmriSettings settings)
    {
        return new JmriClient(settings);
    }

    /// <summary>
    ///     Attempts to reconnect to the JMRI server using the existing client connection.
    /// </summary>
    /// <returns>Returns a result indicating the success or failure of the reconnection attempt.</returns>
    public async Task<IResult> ReconnectAsync() {
        if (_jmriClient == null) {
            return Result.Fail(new Error("No previous connection to reconnect to"));
        }
        
        try {
            await _jmriClient.StopAsync();
            await _jmriClient.InitializeAsync();
            await _jmriClient.StartMonitoringAsync();
            
            _isConnected = true;
            return Result.Ok();
        }
        catch (Exception ex) {
            _isConnected = false;
            return Result.Fail(new Error("Failed to reconnect to JMRI server").CausedBy(ex));
        }
    }

    /// <summary>
    ///     Disconnects from the JMRI server and releases related resources.
    /// </summary>
    /// <returns>A result indicating success or failure of the disconnect operation.</returns>
    public IResult Disconnect() {
        try {
            if (_jmriClient != null) {
                // Unsubscribe from events
                _jmriClient.TurnoutChanged -= OnJmriTurnoutChanged;
                _jmriClient.RouteChanged -= OnJmriRouteChanged;
                _jmriClient.OccupancyChanged -= OnJmriOccupancyChanged;
                _jmriClient.SignalChanged -= OnJmriSignalChanged;
                
                // Stop the client
                _jmriClient.StopAsync().Wait();
                _jmriClient = null;
            }
            
            _isConnected = false;
            return Result.Ok();
        }
        catch (Exception ex) {
            return Result.Fail(new Error("Failed to disconnect from JMRI server").CausedBy(ex));
        }
    }

    public bool IsConnected => _isConnected && _jmriClient != null;

    public IResult SendCmd(string message) {
        try {
            if (!IsConnected) {
                return Result.Fail(new Error("Not connected to JMRI server"));
            }
            
            // Log the command
            OnMessageReceived(new DccMessageArgs("Command", message));
            
            // Since JMRI doesn't have a generic command interface, we'll just log it
            return Result.Ok();
        }
        catch (Exception ex) {
            return Result.Fail(new Error("Failed to send command to JMRI server").CausedBy(ex));
        }
    }

    public IResult SendTurnoutCmd(string dccAddress, bool thrown) {
        try {
            if (!IsConnected || _jmriClient == null) {
                return Result.Fail(new Error("Not connected to JMRI server"));
            }
            
            // Log the command
            OnMessageReceived(new DccMessageArgs("Turnout", $"Setting turnout {dccAddress} to {(thrown ? "THROWN" : "CLOSED")}"));
            
            // Send the command
            _jmriClient.SendTurnoutCommandAsync(dccAddress, thrown).Wait();
            
            return Result.Ok();
        }
        catch (Exception ex) {
            return Result.Fail(new Error("Failed to send turnout command to JMRI server").CausedBy(ex));
        }
    }

    public IResult SendRouteCmd(string dccAddress, bool active) {
        try {
            if (!IsConnected || _jmriClient == null) {
                return Result.Fail(new Error("Not connected to JMRI server"));
            }
            
            // Log the command
            OnMessageReceived(new DccMessageArgs("Route", $"Setting route {dccAddress} to {(active ? "ACTIVE" : "INACTIVE")}"));
            
            // Send the command - JMRI routes are activated by sending the route identifier
            if (active) {
                _jmriClient.SendRouteCommandAsync(dccAddress).Wait();
            }
            
            return Result.Ok();
        }
        catch (Exception ex) {
            return Result.Fail(new Error("Failed to send route command to JMRI server").CausedBy(ex));
        }
    }

    public IResult SendSignalCmd(string dccAddress, SignalAspectEnum aspect) {
        try {
            if (!IsConnected || _jmriClient == null) {
                return Result.Fail(new Error("Not connected to JMRI server"));
            }
            
            // Log the command
            OnMessageReceived(new DccMessageArgs("Signal", $"Setting signal {dccAddress} to aspect {aspect}"));
            
            // Send the command
            _jmriClient.SendSignalCommandAsync(dccAddress, aspect).Wait();
            
            return Result.Ok();
        }
        catch (Exception ex) {
            return Result.Fail(new Error("Failed to send signal command to JMRI server").CausedBy(ex));
        }
    }
    
    #region Event Handlers
    
    private void OnJmriTurnoutChanged(object? sender, TurnoutEventArgs e) {
        // Convert JMRI turnout event to DCC turnout event
        var dccAddress = e.DccAddress?.ToString() ?? e.Identifier;
        var isThrown = e.State.Equals("THROWN", StringComparison.OrdinalIgnoreCase);
        
        // Log the event
        OnMessageReceived(new DccMessageArgs("Turnout", $"Turnout {e.Identifier} state changed to {e.State}"));
        
        // Raise the event
        OnTurnoutMsgReceived(new DccTurnoutArgs(dccAddress, e.Identifier, isThrown));
    }
    
    private void OnJmriRouteChanged(object? sender, RouteEventArgs e) {
        // Convert JMRI route event to DCC route event
        var isActive = e.State.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase);
        
        // Log the event
        OnMessageReceived(new DccMessageArgs("Route", $"Route {e.Identifier} state changed to {e.State}"));
        
        // Raise the event
        OnRouteMsgReceived(new DccRouteArgs(e.Identifier, e.Identifier, isActive));
    }
    
    private void OnJmriOccupancyChanged(object? sender, OccupancyEventArgs e) {
        // Log the event
        OnMessageReceived(new DccMessageArgs("Occupancy", $"Block {e.Identifier} occupancy changed to {(e.IsOccupied ? "OCCUPIED" : "FREE")}"));
        
        // Raise the event
        OnOccupancyMsgReceived(new DccOccupancyArgs(e.Identifier, e.Identifier, e.IsOccupied));
    }
    
    private void OnJmriSignalChanged(object? sender, SignalEventArgs e) {
        // Log the event
        OnMessageReceived(new DccMessageArgs("Signal", $"Signal {e.Identifier} aspect changed to {e.State}"));
        
        // Raise the event
        var dccAddress = e.DccAddress?.ToString() ?? e.Identifier;
        OnSignalMsgReceived(new DccSignalArgs(dccAddress, e.Identifier, e.Aspect));
    }
    
    #endregion
}
