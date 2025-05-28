using System.Data;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using DCCCommon.Common;
using DCCClients.WiThrottle.WiThrottle.Client.Commands;
using DCCClients.WiThrottle.WiThrottle.Client.Events;
using DCCClients.WiThrottle.WiThrottle.Client.Messages;
using DCCClients.WiThrottle.WiThrottle.Helpers;
using Result = DCCCommon.Common.Result;
using Timer = System.Timers.Timer;

namespace DCCClients.WiThrottle.WiThrottle.Client;

public class Client {
    private readonly WithrottleSettings _withrottleSettings;
    private TcpClient? _client;
    private Timer? _heartbeatTimer;
    private NetworkStream? _stream;
    public Client(string address, int port) : this(new WithrottleSettings(address, port)) { }
    public Client(string name, string address, int port) : this(new WithrottleSettings(name, address, port)) { }

    public Client(WithrottleSettings withrottleSettings) {
        _withrottleSettings = withrottleSettings;
    }

    public bool IsRunning { get; private set; }

    public bool Echo { get; set; } = true;
    public event Action<IClientEvent>? DataEvent;
    public event Action<IClientEvent>? ConnectionEvent;

    public async Task<IResult> ConnectAsync() {
        if (IsRunning) Stop();
        try {
            await EstablishConnectionWithRetries();
            var listenThread = new Thread(Listen);
            listenThread.Start();
            return Result.Ok();
        } catch (Exception ex) {
            return Result.Fail(new Error("Failed to connect").CausedBy(ex));
        }
    }

    private async Task EstablishConnectionWithRetries(int maxRetries = 3, int delayMilliseconds = 2000) {
        CloseConnection();

        var retryCount = 0;
        while (retryCount < maxRetries) {
            try {
                await EstablishConnection(); // Attempt to establish a connection
                return;                      // Exit the loop on success
            } catch (Exception ex) {
                retryCount++;
                Console.WriteLine($"Failed to connect: {ex.Message} Retrying... {retryCount}/{maxRetries}");
                if (retryCount >= maxRetries) {
                    // Log and rethrow the exception if max retries are reached
                    Console.WriteLine($"Failed to connect after {maxRetries} attempts: {ex.Message}");
                    throw;
                }

                Console.WriteLine($"Retrying connection... Attempt {retryCount}/{maxRetries}");
                await Task.Delay(delayMilliseconds); // Wait before retrying
            }
        }
    }

    /// <summary>
    ///     Used to connect, or try again, to get a connection
    /// </summary>
    private async Task EstablishConnection() {
        Console.WriteLine("Establishing connection...");
        if (_client is not null && _client.Connected) return;
        if (_client is not null) CloseConnection();

        _client = new TcpClient();
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) {
            await _client.ConnectAsync(_withrottleSettings.Address, _withrottleSettings.Port, cts.Token);
        }
        _stream = _client.GetStream();
        IsRunning = true;
        Console.WriteLine("Connection established.");
    }

    /// <summary>
    ///     Listen for incomming messages and event them via the DataReceived event to the caller.
    /// </summary>
    private void Listen() {
        StringBuilder buffer = new();
        var bytes = new byte[256];
        SendWakeUpMessages();

        while (IsRunning) {
            try {
                if (_stream is { DataAvailable: true } && _client is { Connected: true }) {
                    var bytesRead = 0;
                    try {
                        bytesRead = _stream.Read(bytes, 0, bytes.Length);
                    } catch (ObjectDisposedException) {
                        break;
                    }

                    if (bytesRead != 0) {
                        var data = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                        buffer.Append(data);

                        if (Terminators.HasTerminator(buffer)) {
                            foreach (var command in Terminators.GetMessagesAndLeaveIncomplete(buffer)) {
                                ProcessMessage(command);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Listener encountered an error: {ex.Message}");
                EstablishConnectionWithRetries().Wait();
                if (_client is null) {
                    ConnectionEvent?.Invoke(new ConnectionEvent(ex.Message, GetConnectionState(), false));
                    break;
                }
            }
            Thread.Sleep(100);
        }
        CloseConnection();
        ConnectionEvent?.Invoke(new ConnectionEvent("Connection closed", GetConnectionState(), false));
    }

    private void CloseConnection() {
        Console.WriteLine("Closing connection...");
        IsRunning = false;
        _client?.Close();
        _client = null;
        _stream = null;
        _heartbeatTimer?.Stop();
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    private void ProcessMessage(string message) {
        var clientMsg = MessageProcessor.Interpret(message);

        switch (clientMsg) {
        case MsgQuit quit:
            IsRunning = false;
            break;

        case MsgHeartbeat heartbeat:
            StopHeartbeatTimer();
            StartHeartbeatTimer(heartbeat.HeartbeatSeconds);
            break;

        default:
            foreach (var clientEvent in clientMsg.FoundEvents) {
                OnClientEventOccurred(clientEvent);
            }

            break;
        }
    }

    private void SendWakeUpMessages() {
        // To initialise a connection to a WiThrottle service we need to send the following messages
        // ------------------------------------------------------------------------------------------
        SendMessage(_withrottleSettings.GetNameMessage);
        SendMessage(_withrottleSettings.GetHardwareMessage);
        SendMessage("*+");
    }

    private void SendShutdownMessages() {
        // To initialise a connection to a WiThrottle service we need to send the following messages
        // ------------------------------------------------------------------------------------------
        SendMessage("*-");
        SendMessage("Q");
    }

    public void Disconnect() {
        SendShutdownMessages();
        Stop();
    }

    public void SendMessage(IClientCmd command) {
        SendMessage(command.Command);
    }

    public void SendMessage(string message) {
        message = message.WithTerminator();
        if (Echo) OnClientEventOccurred(new MessageEvent("Command Sent", message));

        try {
            if (_stream is { CanWrite: true }) {
                var data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
        } catch (Exception ex) {
            EstablishConnectionWithRetries().Wait();
            if (_client is null) {
                ConnectionEvent?.Invoke(new ConnectionEvent(ex.Message, GetConnectionState(), false));
                CloseConnection();
            }
        }
    }

    private ConnectionState GetConnectionState() {
        if (_client is null) return ConnectionState.Closed;
        if (!_client.Connected) return ConnectionState.Closed;
        if (_stream is null) return ConnectionState.Open;
        if (!_stream.CanRead) return ConnectionState.Open;
        if (!_stream.CanWrite) return ConnectionState.Open;
        return ConnectionState.Open;
    }

    /// <summary>
    ///     Shutdown the connection to the WiThrottle Service and clean up.
    /// </summary>
    public void Stop() {
        CloseConnection();
    }

    private void StartHeartbeatTimer(int secs) {
        _heartbeatTimer = new Timer(secs * 1000);
        _heartbeatTimer.Elapsed += HeartbeatTimerOnElapsed;
        _heartbeatTimer.AutoReset = true;
        _heartbeatTimer.Start();
    }

    private void HeartbeatTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        SendMessage("*");
    }

    private void StopHeartbeatTimer() {
        if (_heartbeatTimer is not null) {
            _heartbeatTimer.Elapsed -= HeartbeatTimerOnElapsed;
            _heartbeatTimer.Stop();
        }
    }

    protected void OnClientEventOccurred(IClientEvent clientEvent) {
        DataEvent?.Invoke(clientEvent);
    }
}