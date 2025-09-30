// using System.Data;
// using System.Diagnostics;
// using System.Net.Sockets;
// using System.Text;
// using System.Timers;
// using DCCPanelController.Clients.WiThrottle.Client;
// using DCCPanelController.Clients.WiThrottle.Client.Commands;
// using DCCPanelController.Clients.WiThrottle.Client.Events;
// using DCCPanelController.Clients.WiThrottle.Client.Messages;
// using DCCPanelController.Clients.WiThrottle.Helpers;
// using DCCPanelController.Helpers;
// using Timer = System.Timers.Timer;
//
// namespace DCCPanelController.Clients.WiThrottle;
//
// public class WiThrottleClient {
//     private readonly string _address;
//
//     private readonly string _name;
//     private readonly int _port;
//     private TcpClient? _client;
//     private Timer? _heartbeatTimer;
//     private NetworkStream? _stream;
//
//     public WiThrottleClient(string address, int port = 12090) : this("", address, port) { }
//
//     public WiThrottleClient(string name, string address, int port = 12090) {
//         _name = name;
//         _address = address;
//         _port = port;
//     }
//
//     public bool IsRunning { get; private set; }
//     public Guid Id { get; init; } = Guid.NewGuid();
//     public string GetNameMessage => $"N{_name}";
//     public string GetHardwareMessage => $"HU{Id.ToString()}";
//     public bool Echo { get; set; } = true;
//     public event Action<IClientEvent>? DataEvent;
//     public event Action<IClientEvent>? ConnectionEvent;
//
//     public async Task<IResult> ConnectAsync() {
//         if (IsRunning) Stop();
//         try {
//             await EstablishConnectionWithRetries();
//             var listenThread = new Thread(Listen);
//             listenThread.Start();
//             return Result.Ok();
//         } catch (Exception ex) {
//             return Result.Fail(ex,"Failed to connect");
//         }
//     }
//
//     private async Task EstablishConnectionWithRetries(int maxRetries = 3, int delayMilliseconds = 2000) {
//         CloseConnection();
//
//         var retryCount = 0;
//         while (retryCount < maxRetries) {
//             try {
//                 await EstablishConnection(); // Attempt to establish a connection
//                 return;                      // Exit the loop on success
//             } catch (Exception ex) {
//                 retryCount++;
//                 Debug.WriteLine($"Failed to connect: {ex.Message} Retrying... {retryCount}/{maxRetries}");
//                 if (retryCount >= maxRetries) {
//                     // Log and rethrow the exception if max retries are reached
//                     Debug.WriteLine($"Failed to connect after {maxRetries} attempts: {ex.Message}");
//                     throw;
//                 }
//
//                 Debug.WriteLine($"Retrying connection... Attempt {retryCount}/{maxRetries}");
//                 await Task.Delay(delayMilliseconds); // Wait before retrying
//             }
//         }
//     }
//
//     /// <summary>
//     ///     Used to connect, or try again, to get a connection
//     /// </summary>
//     private async Task EstablishConnection() {
//         Debug.WriteLine("Establishing connection...");
//         if (_client is not null && _client.Connected) return;
//         if (_client is not null) CloseConnection();
//
//         _client = new TcpClient();
//         using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) {
//             await _client.ConnectAsync((string)_address, _port, cts.Token);
//         }
//         _stream = _client.GetStream();
//         IsRunning = true;
//         Debug.WriteLine("Connection established.");
//     }
//
//     /// <summary>
//     ///     Listen for incomming messages and event them via the DataReceived event to the caller.
//     /// </summary>
//     private void Listen() {
//         StringBuilder buffer = new();
//         var bytes = new byte[256];
//         SendWakeUpMessages();
//
//         while (IsRunning) {
//             try {
//                 if (_stream is { DataAvailable: true } && _client is { Connected: true }) {
//                     var bytesRead = 0;
//                     try {
//                         bytesRead = _stream.Read(bytes, 0, bytes.Length);
//                     } catch (ObjectDisposedException) {
//                         break;
//                     }
//
//                     if (bytesRead != 0) {
//                         var data = Encoding.ASCII.GetString(bytes, 0, bytesRead);
//                         buffer.Append(data);
//
//                         if (Terminators.HasTerminator(buffer)) {
//                             foreach (var command in Terminators.GetMessagesAndLeaveIncomplete(buffer)) {
//                                 ProcessMessage(command);
//                             }
//                         }
//                     }
//                 }
//             } catch (Exception ex) {
//                 Debug.WriteLine($"Listener encountered an error: {ex.Message}");
//                 EstablishConnectionWithRetries().Wait();
//                 if (_client is null) {
//                     ConnectionEvent?.Invoke(new ConnectionEvent(ex.Message, GetConnectionState(), false));
//                     break;
//                 }
//             }
//             Thread.Sleep(100);
//         }
//         CloseConnection();
//         ConnectionEvent?.Invoke(new ConnectionEvent("Connection closed", GetConnectionState(), false));
//     }
//
//     private void CloseConnection() {
//         Debug.WriteLine("Closing connection...");
//         IsRunning = false;
//         _client?.Close();
//         _client = null;
//         _stream = null;
//         _heartbeatTimer?.Stop();
//         _heartbeatTimer?.Dispose();
//         _heartbeatTimer = null;
//     }
//
//     private void ProcessMessage(string message) {
//         var clientMsg = MessageProcessor.Interpret(message);
//
//         switch (clientMsg) {
//         case MsgQuit quit:
//             IsRunning = false;
//             break;
//
//         case MsgHeartbeat heartbeat:
//             StopHeartbeatTimer();
//             StartHeartbeatTimer(heartbeat.HeartbeatSeconds);
//             break;
//
//         default:
//             foreach (var clientEvent in clientMsg.FoundEvents) {
//                 OnClientEventOccurred(clientEvent);
//             }
//
//             break;
//         }
//     }
//
//     private void SendWakeUpMessages() {
//         // To initialise a connection to a WiThrottle service we need to send the following messages
//         // ------------------------------------------------------------------------------------------
//         SendMessage(GetNameMessage);
//         SendMessage(GetHardwareMessage);
//         SendMessage("*+");
//     }
//
//     private void SendShutdownMessages() {
//         // To initialise a connection to a WiThrottle service we need to send the following messages
//         // ------------------------------------------------------------------------------------------
//         SendMessage("*-");
//         SendMessage("Q");
//     }
//
//     public void Disconnect() {
//         SendShutdownMessages();
//         Stop();
//     }
//
//     public void SendMessage(IClientCmd command) {
//         SendMessage(command.Command);
//     }
//
//     public void SendMessage(string message) {
//         message = message.WithTerminator();
//         if (Echo) OnClientEventOccurred(new MessageEvent("Command Sent", message));
//
//         try {
//             if (_stream is { CanWrite: true }) {
//                 var data = Encoding.UTF8.GetBytes(message);
//                 _stream.Write(data, 0, data.Length);
//             }
//         } catch (Exception ex) {
//             EstablishConnectionWithRetries().Wait();
//             if (_client is null) {
//                 ConnectionEvent?.Invoke(new ConnectionEvent(ex.Message, GetConnectionState(), false));
//                 CloseConnection();
//             }
//         }
//     }
//
//     private ConnectionState GetConnectionState() {
//         if (_client is null) return ConnectionState.Closed;
//         if (!_client.Connected) return ConnectionState.Closed;
//         if (_stream is null) return ConnectionState.Open;
//         if (!_stream.CanRead) return ConnectionState.Open;
//         if (!_stream.CanWrite) return ConnectionState.Open;
//         return ConnectionState.Open;
//     }
//
//     /// <summary>
//     ///     Shutdown the connection to the WiThrottle Service and clean up.
//     /// </summary>
//     public void Stop() {
//         CloseConnection();
//     }
//
//     private void StartHeartbeatTimer(int secs) {
//         _heartbeatTimer = new Timer(secs * 1000);
//         _heartbeatTimer.Elapsed += HeartbeatTimerOnElapsed;
//         _heartbeatTimer.AutoReset = true;
//         _heartbeatTimer.Start();
//     }
//
//     private void HeartbeatTimerOnElapsed(object? sender, ElapsedEventArgs e) {
//         SendMessage("*");
//     }
//
//     private void StopHeartbeatTimer() {
//         if (_heartbeatTimer is not null) {
//             _heartbeatTimer.Elapsed -= HeartbeatTimerOnElapsed;
//             _heartbeatTimer.Stop();
//         }
//     }
//
//     protected void OnClientEventOccurred(IClientEvent clientEvent) {
//         DataEvent?.Invoke(clientEvent);
//     }
// }