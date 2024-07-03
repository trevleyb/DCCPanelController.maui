using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DCCPanelController.Model;
using DCCWiThrottleClient.Client;
using DCCWiThrottleClient.Client.Messages;

namespace DCCPanelController.Services;

public class ConnectionService {
    
    private Turnouts _turnouts;
    private Client _client;
    
    public void Connect(WiServer wiServer) {
        _turnouts = [];
        _client = new Client(wiServer.IpAddress, wiServer.Port, _turnouts);
        _client.MessageProcessed += ClientOnMessageProcessed;
        _turnouts.CollectionChanged += TurnoutsOnCollectionChanged;
        var didConnect = _client.Connect();
        if (didConnect.Failed) throw new Exception("Unable to connect to the WiThrottle Client Defined.");
    }

    private void ClientOnMessageProcessed(IClientMsg obj) {
        Console.WriteLine(obj.ActionTaken);
    }

    private void TurnoutsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        Console.WriteLine(e.Action);
    }
}