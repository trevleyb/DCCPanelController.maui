// using CommunityToolkit.Mvvm.ComponentModel;
// using DCCPanelController.Model;
// using DCCWithrottleClient.Client;
// using DCCWithrottleClient.Client.Commands;
// using DCCWithrottleClient.Client.Events;
// using Route = DCCPanelController.Model.DataModel.Route;
// using RouteStateEnum = DCCWithrottleClient.Client.RouteStateEnum;
// using Turnout = DCCPanelController.Model.DataModel.Turnout;
// using TurnoutStateEnum = DCCWithrottleClient.Client.TurnoutStateEnum;
// using WiServer = DCCPanelController.Model.DataModel.WiServer;
//
// namespace DCCPanelController.Services;
//
// public partial class ConnectionService : ObservableObject {
//     private Client? _client;
//
//     [ObservableProperty] private bool _isConnected;
//     private RoutesService? _routesService;
//     private TurnoutsService? _turnoutsService;
//
//     public event Action<string>? MessageRecieved;
//
//     public void Connect(WiServer wiServer) {
//         // Get the Route and Turnout Services so we can Update the list of Turnouts and Routes 
//         // as we get data back from the WiServer. 
//         // ------------------------------------------------------------------------------------
//         _turnoutsService = MauiProgram.ServiceHelper.GetService<TurnoutsService>();
//         _routesService = MauiProgram.ServiceHelper.GetService<RoutesService>();
//         ArgumentNullException.ThrowIfNull(_turnoutsService);
//         ArgumentNullException.ThrowIfNull(_routesService);
//
//         _client = new Client(wiServer.IpAddress, wiServer.Port);
//         _client.ConnectionEvent += ClientOnConnectionEvent;
//         _client.ConnectionError += ClientOnConnectionError;
//         if (!_client.Connect().Success) throw new Exception("Unable to connect to the WiThrottle Client Defined.");
//         IsConnected = true;
//     }
//
//     public void Disconnect() {
//         if (_client != null) {
//             _client.ConnectionEvent -= ClientOnConnectionEvent;
//             _client.ConnectionError -= ClientOnConnectionError;
//             _client.Disconnect();
//         }
//
//         IsConnected = false;
//     }
//
//     private void ClientOnConnectionError(string obj) {
//         Console.WriteLine("Connection Error: " + obj);
//     }
//
//     //public void SendTurnoutStateChangeCommand(string id, Model.DataModel.TurnoutStateEnum state) {
//     //    _client?.SendMessage(new TurnoutCommand(id, state == Model.DataModel.TurnoutStateEnum.Closed ? TurnoutStateEnum.Closed : TurnoutStateEnum.Thrown));
//     //}
//
//     public void SendTurnoutStateToggleCommand(string id) {
//         _client?.SendMessage(new TurnoutCommand(id, TurnoutStateEnum.Toggle));
//     }
//
//     //public void SendRouteStateChangeCommand(string id, Model.DataModel.RouteStateEnum state) {
//     //    _client?.SendMessage(new RouteCommand(id));
//     //}
//
//     private void ClientOnConnectionEvent(IClientEvent clientEvent) {
//         MessageRecieved?.Invoke(clientEvent?.ToString() ?? "Unknown Message");
//
//         switch (clientEvent) {
//         case MessageEvent message:
//             break;
//
//         case RosterEvent roster:
//             break;
//
//         case RouteEvent route:
//             UpdateRoute(route);
//             break;
//
//         case TurnoutEvent turnout:
//             UpdateTurnout(turnout);
//             break;
//
//         case FastClockEvent clock:
//             break;
//         }
//     }
//
//     private void UpdateTurnout(TurnoutEvent turnout) {
//         var found = _turnoutsService?.GetTurnoutByIdAsync(turnout.SystemName).Result;
//
//         if (found == null) {
//             _turnoutsService?.AddTurnoutAsync(new Turnout {
//                 Id = turnout.SystemName,
//                 Name = turnout.UserName,
//                 IsEditable = false,
//                 //State = turnout.StateEnum switch {
//                 //    TurnoutStateEnum.Closed => Model.DataModel.TurnoutStateEnum.Closed,
//                 //    TurnoutStateEnum.Thrown => Model.DataModel.TurnoutStateEnum.Thrown,
//                 //    _                       => Model.DataModel.TurnoutStateEnum.Unknown
//                // }
//             });
//         } else {
//             found.Id = turnout.SystemName;
//             found.IsEditable = false;
//
//             //found.State = turnout.StateEnum switch {
//             //    TurnoutStateEnum.Closed => Model.DataModel.TurnoutStateEnum.Closed,
//             //    TurnoutStateEnum.Thrown => Model.DataModel.TurnoutStateEnum.Thrown,
//             //    _                       => Model.DataModel.TurnoutStateEnum.Unknown
//             //};
//
//             if (!string.IsNullOrWhiteSpace(turnout.UserName)) {
//                 found.Name = turnout.UserName;
//             }
//         }
//     }
//
//     private void UpdateRoute(RouteEvent route) {
//         var found = _routesService?.GetRouteByIdAsync(route.SystemName).Result;
//
//         if (found == null) {
//             _routesService?.AddRouteAsync(new Route {
//                 Id = route.SystemName,
//                 Name = route.UserName,
//                 //State = route.StateEnum switch {
//                 //    RouteStateEnum.Active   => Model.DataModel.RouteStateEnum.Active,
//                 //    RouteStateEnum.Inactive => Model.DataModel.RouteStateEnum.Inactive,
//                 //    _                       => Model.DataModel.RouteStateEnum.Unknown
//                 //}
//             });
//         } else {
//             found.Id = route.SystemName;
//
//             //found.State = route.StateEnum switch {
//                 //RouteStateEnum.Active   => Model.DataModel.RouteStateEnum.Active,
//                 //RouteStateEnum.Inactive => Model.DataModel.RouteStateEnum.Inactive,
//                 //_                       => Model.DataModel.RouteStateEnum.Unknown
//             //};
//
//             if (!string.IsNullOrWhiteSpace(route.UserName)) {
//                 found.Name = route.UserName;
//             }
//         }
//     }
// }