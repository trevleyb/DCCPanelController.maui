using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCWiThrottleClient.Client;
using DCCWiThrottleClient.Client.Messages;
using Route = DCCWiThrottleClient.Client.Route;
using RouteStateEnum = DCCWiThrottleClient.Client.RouteStateEnum;
using Turnout = DCCWiThrottleClient.Client.Turnout;
using TurnoutStateEnum = DCCWiThrottleClient.Client.TurnoutStateEnum;

namespace DCCPanelController.Services;

public partial class ConnectionService : ObservableObject {
    
    private Turnouts _turnouts = new Turnouts();     // Turnouts Managed by the Client 
    private Routes _routes = new Routes();         // Routes Managed by the Client
    private Client _client;
    private TurnoutsService? _turnoutsService;
    private RoutesService? _routesService;
    
    // Add an event that will be raised when a new message is processed
    public event Action<IClientMsg>? MessageProcessed;

    [ObservableProperty]
    private bool _isConnected = false;
    
    public void Connect(WiServer wiServer) {
        _turnoutsService =  App.ServiceProvider?.GetService<TurnoutsService>();
        ArgumentNullException.ThrowIfNull(_turnoutsService);
        
        _routesService =  App.ServiceProvider?.GetService<RoutesService>();
        ArgumentNullException.ThrowIfNull(_routesService);

        _turnouts = [];
        _routes   = [];
        
        _client = new Client(wiServer.IpAddress, wiServer.Port, _turnouts, _routes);
        _client.MessageProcessed     += ClientOnMessageProcessed;
        _client.ConnectionError      += ClientOnConnectionError;
        _client.DataReceived         += ClientOnDataReceived;
        _turnouts.CollectionChanged  += TurnoutsOnCollectionChanged;
        _turnouts.EntityChangedEvent += TurnoutsOnEntityChangedEvent;
        _routes.CollectionChanged    += RoutesOnCollectionChanged;
        _routes.EntityChangedEvent   += RoutesOnEntityChangedEvent;
        var didConnect = _client.Connect();
        if (didConnect.Failed) throw new Exception("Unable to connect to the WiThrottle Client Defined.");
        IsConnected = true;
    }
    
    public void Disconnect() {
        _turnouts.CollectionChanged  -= TurnoutsOnCollectionChanged;
        _turnouts.EntityChangedEvent -= TurnoutsOnEntityChangedEvent;
        _routes.CollectionChanged    -= RoutesOnCollectionChanged;
        _routes.EntityChangedEvent   -= RoutesOnEntityChangedEvent;
        _client.MessageProcessed     -= ClientOnMessageProcessed;
        _client.ConnectionError      -= ClientOnConnectionError;
        _client.DataReceived         -= ClientOnDataReceived;
        _client.Disconnect();
        IsConnected = false;
    }

    private void ClientOnDataReceived(string obj) {
        Console.WriteLine(obj.ToString());
    }

    private void ClientOnConnectionError(string obj) {
        Console.WriteLine("Connection Error: "+obj.ToString());
    }
    
    private void ClientOnMessageProcessed(IClientMsg obj) {
        MessageProcessed?.Invoke(obj);
    }

    public void SendTurnoutStateChangeCommand(string ID, Model.TurnoutStateEnum state) {
        // This should be finished and managed as message objects - TODO
        //PTATLT304
        var message = $"PTA{(state == Model.TurnoutStateEnum.Closed ? "C" : "T")}{ID}";
        _client.SendMessage(message);
    }

    public void SendRouteStateChangeCommand(string ID, Model.RouteStateEnum state) {
        // This should be finished and managed as message objects - TODO
        //PTATLT304
        var message = $"PRA{(state == Model.RouteStateEnum.Active ? "2" : "4")}{ID}";
        _client.SendMessage(message);
    }
    
    /// <summary>
    /// This is called whenever we change a Turnout (add or update). Use this to then change the
    /// Panel Turnouts List. Update the list s that we do not override any existing items
    /// and add new ones if they do not exist. 
    /// </summary>
    private void TurnoutsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {

        if (e.NewItems != null) {
            foreach (var item in e.NewItems) {
                if (item is Turnout { } turnout) {
                    switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        TurnoutsOnEntityChangedEvent(turnout);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        _turnoutsService?.DeleteTurnoutAsync(turnout.Name);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        TurnoutsOnEntityChangedEvent(turnout);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                    }
                }
            }
        }
    }

    private void TurnoutsOnEntityChangedEvent(Turnout obj) {
        var found = _turnoutsService?.GetTurnoutByIdAsync(obj.Name).Result;
        if (found == null) {
            _turnoutsService?.AddTurnoutAsync(new Model.Turnout() {
                Id = obj.Name,
                Name = obj.UserName,
                State = obj.StateEnum switch {
                    TurnoutStateEnum.Closed => Model.TurnoutStateEnum.Closed,
                    TurnoutStateEnum.Thrown => Model.TurnoutStateEnum.Thrown,
                    _                       => Model.TurnoutStateEnum.Unknown
                }
            });
        } else { 
            found.Id = obj.Name;
            found.Name = obj.UserName;
            found.State = obj.StateEnum switch {
                TurnoutStateEnum.Closed => Model.TurnoutStateEnum.Closed,
                TurnoutStateEnum.Thrown => Model.TurnoutStateEnum.Thrown,
                _ => Model.TurnoutStateEnum.Unknown,
            };
        }
    }
    
    /// <summary>
    /// This is called whenever we change a Route (add or update). Use this to then change the
    /// Panel Routes List. Update the list so that we do not override any existing items
    /// and add new ones if they do not exist. 
    /// </summary>
    private void RoutesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var item in e.NewItems) {
                if (item is Route { } route) {
                    switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        RoutesOnEntityChangedEvent(route);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        _routesService?.DeleteRouteAsync(route.Name);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        RoutesOnEntityChangedEvent(route);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                    }
                }
            }
        }
    }

    private void RoutesOnEntityChangedEvent(Route obj) {
        var found = _routesService?.GetRouteByIdAsync(obj.Name).Result;
        if (found == null) {
            _routesService?.AddRouteAsync(new Model.Route() {
                Id = obj.Name,
                Name = obj.UserName,
                State = obj.StateEnum switch {
                    RouteStateEnum.Active   => Model.RouteStateEnum.Active,
                    RouteStateEnum.Inactive => Model.RouteStateEnum.Inactive,
                    _                       => Model.RouteStateEnum.Unknown
                }
            });
        } else {
            found.Id = obj.Name;
            found.Name = obj.UserName;
            found.State = obj.StateEnum switch {
                RouteStateEnum.Active   => Model.RouteStateEnum.Active,
                RouteStateEnum.Inactive => Model.RouteStateEnum.Inactive,
                _                       => Model.RouteStateEnum.Unknown
            };
        }
    }
}