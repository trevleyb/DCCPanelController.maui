using DCCClients;
using DCCPanelController.Models;
using DCCPanelController.Helpers.Result;
    
namespace DCCPanelController.Services;

public class ConnectionService {
    
    private IDccClient? _dccClient;
    public IDccClient GetClient(Connection? connection = null) {
        
        // Allow null as a parameter if we know we have already setup the connection
        // This will then return the active connection.
        // --------------------------------------------------------------------------
        if (_dccClient != null) return _dccClient;
        
        // If we don't have a connection setup, and we did not provide settings, then we error
        // --------------------------------------------------------------------------
        if (connection?.Settings is null) throw new ArgumentNullException(nameof(connection),"No Connection Details provided.");
        
        
        // Attempt to connect to the Service provided and return the client
        // --------------------------------------------------------------------------
        _dccClient = DccClientFactory.Create(connection.Settings);
        var result = _dccClient.ConnectAsync().Result;
        if (result.IsFailure) _dccClient = new DccInvalidClient(connection.Settings);
        return _dccClient;  
    }

    public void CloseClient() {
        _dccClient?.Disconnect();
        _dccClient = null;
    }
}