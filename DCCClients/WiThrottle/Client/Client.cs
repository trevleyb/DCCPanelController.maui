using System.Net.Sockets;
using System.Text;
using System.Timers;
using DCCClients.Common;
using DCCClients.WiThrottle.Client.Commands;
using DCCClients.WiThrottle.Client.Events;
using DCCClients.WiThrottle.Client.Messages;
using DCCClients.WiThrottle.Helpers;
using Result = DCCClients.Common.Result;
using Timer = System.Timers.Timer;

namespace DCCClients.WiThrottle.Client;

public class Client {
    private TcpClient? _client;
    private NetworkStream? _stream;
    private Timer? _heartbeatTimer;
    private bool _running;
    private readonly WithrottleSettings _withrottleSettings;

    public bool IsRunning => _running;
    public Client(string address, int port) : this(new WithrottleSettings(address, port)) { }
    public Client(string name, string address, int port) : this(new WithrottleSettings(name, address, port)) { }

    public Client(WithrottleSettings withrottleSettings) {
        _withrottleSettings = withrottleSettings;
    }

    public bool Echo { get; set; } = true;
    public event Action<IClientEvent>? ConnectionEvent;
    public event Action<string>? ConnectionError;

    public async Task<IResult> ConnectAsync()
    {
        if (_running) Stop();
        try
        {
            _client = new TcpClient();

            // Attempt to connect asynchronously with a timeout
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) {
                await _client.ConnectAsync(_withrottleSettings.Address, _withrottleSettings.Port, cts.Token); 
            }

            _stream = _client.GetStream();
            _running = true;

            // Start the listener as an asynchronous background task
            _ = Task.Run(ListenAsync);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Failed to connect").CausedBy(ex));
        }
    }

    private async Task ListenAsync() {

        if (_client is null || _stream is null) throw new InvalidOperationException("Client or stream is null");
        
        StringBuilder buffer = new();
        var bytes = new byte[256];

        SendWakeUpMessages();
        try {
            while (_running && _client.Connected) {
                var bytesRead = await _stream.ReadAsync(bytes, 0, buffer.Length);
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
        }
        catch (Exception ex) {
            Console.WriteLine($"Listener encountered an error: {ex.Message}");
            ConnectionError?.Invoke(ex.Message);
        }
        finally {
            Stop();
        }
    }

    
    /// <summary>
    ///     Listen for incomming messages and event them via the DataReceived event to the caller.
    /// </summary>
    private void ListenOld() {
        StringBuilder buffer = new();
        var bytes = new byte[256];
        SendWakeUpMessages();

        while (_running) {
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
                // Just ignore any exceptions for now, but this should raise events to say 
                // that we have an issue so that we can try to re-establish the connection
                ConnectionError?.Invoke(ex.Message);
            }

            Thread.Sleep(100);
        }
    }

    private void ProcessMessage(string message) {
        var clientMsg = MessageProcessor.Interpret(message);

        switch (clientMsg) {
        case MsgQuit quit:
            _running = false;
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
        if (Echo) OnClientEventOccurred(new MessageEvent("Command", message));

        try {
            if (_stream is { CanWrite: true }) {
                var data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
        } catch (Exception ex) {
            // Just ignore any exceptions for now, but this should raise events to say 
            // that we have an issue so that we can try to re-establish the connection
            ConnectionError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    ///     Shutdown the connection to the WiThrottle Service and clean up.
    /// </summary>
    public void Stop() {
        _running = false;
        if (_stream is { } stream) stream.Close();
        if (_client is { } client) client.Close();
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
        ConnectionEvent?.Invoke(clientEvent);
    }
}