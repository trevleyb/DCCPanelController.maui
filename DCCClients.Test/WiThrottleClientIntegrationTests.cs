using DCCCommon.Common;
using DCCCommon.Events;
using DCCClients.WiThrottle;
using DCCClients.WiThrottle.WiThrottle.Client;

namespace DCCClients.Test;

[TestFixture]
[Category("Integration")]
public class WiThrottleClientIntegrationTests {
    private DccWiThrottleClient _client = null!;
    private WithrottleSettings _settings = null!;

    // Event tracking variables
    private List<DccTurnoutArgs> _receivedTurnoutEvents = new();
    private List<DccRouteArgs> _receivedRouteEvents = new();
    private List<DccMessageArgs> _receivedMessages = new();
    private List<DccStateChangedArgs> _receivedErrors = new();

    [OneTimeSetUp]
    public void OneTimeSetUp() {
        var wiThrottleAddress = "localhost";    //Environment.GetEnvironmentVariable("WITHROTTLE_SERVER_ADDRESS");
        var wiThrottlePortStr = "12090";        //Environment.GetEnvironmentVariable("WITHROTTLE_SERVER_PORT");

        if (string.IsNullOrEmpty(wiThrottleAddress) || string.IsNullOrEmpty(wiThrottlePortStr)) {
            Assert.Ignore("WITHROTTLE_SERVER_ADDRESS or WITHROTTLE_SERVER_PORT environment variables not set. Skipping integration tests.");
        }

        if (!int.TryParse(wiThrottlePortStr, out var wiThrottlePort)) {
            Assert.Ignore("WITHROTTLE_SERVER_PORT environment variable is not a valid integer. Skipping integration tests.");
        }
        _settings = new WithrottleSettings("IntegrationTest", wiThrottleAddress, wiThrottlePort);
    }

    [SetUp]
    public void Setup() {
        _client = new DccWiThrottleClient(_settings);

        // Clear event tracking
        _receivedTurnoutEvents = new List<DccTurnoutArgs>();
        _receivedRouteEvents = new List<DccRouteArgs>();
        _receivedMessages = new List<DccMessageArgs>();
        _receivedErrors = new List<DccStateChangedArgs>();

        // Subscribe to events
        _client.TurnoutMsgReceived += (sender, args) => _receivedTurnoutEvents.Add(args);
        _client.RouteMsgReceived += (sender, args) => _receivedRouteEvents.Add(args);
        _client.MessageReceived += (sender, args) => _receivedMessages.Add(args);
        _client.ConnectionStateChanged += (sender, args) => _receivedErrors.Add(args);
    }

    [TearDown]
    public void TearDown() {
        _client.DisconnectAsync();
    }

    [Test]
    public async Task ConnectAsync_ShouldConnectToWiThrottleServer() {
        // Act
        var result = await _client.ConnectAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Connection should succeed");
        Assert.That(_client.IsConnected, Is.True, "Client should be connected");

        // Wait a bit to receive initial events
        await Task.Delay(2000);

        // We should have received some messages at minimum
        Assert.That(_receivedMessages, Is.Not.Empty, "Should receive some messages");
    }

    [Test]
    public async Task ReconnectAsync_ShouldReconnectAfterDisconnect() {
        // Arrange
        await _client.ConnectAsync();
        await _client.DisconnectAsync();

        // Clear events
        _receivedMessages.Clear();

        // Act
        var result = await _client.ReconnectAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Reconnection should succeed");
        Assert.That(_client.IsConnected, Is.True, "Client should be connected");

        // Wait a bit to receive initial events
        await Task.Delay(2000);

        // We should have received some messages at minimum
        Assert.That(_receivedMessages, Is.Not.Empty, "Should receive some messages after reconnect");
    }

    [Test]
    public async Task SendCmd_ShouldSendCommandToWiThrottleServer() {
        // Arrange
        await _client.ConnectAsync();
        await Task.Delay(1000);

        // Clear events
        _receivedMessages.Clear();

        // Act - Send a heartbeat command
        var result = await _client.SendCmdAsync("*");

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Sending command should succeed");

        // Wait for response
        await Task.Delay(1000);

        // Should have received a message about the command
        Assert.That(_receivedMessages, Has.Count.GreaterThan(0), "Should receive message about command");
    }

    [Test]
    public async Task SendTurnoutCmd_ShouldSendCommandToWiThrottleServer() {
        // Arrange - Connect and wait for initial events
        await _client.ConnectAsync();
        await Task.Delay(2000);

        // Get a turnout ID from received events if available
        string turnoutId = "LT1"; // Default fallback
        if (_receivedTurnoutEvents.Count > 0) {
            turnoutId = _receivedTurnoutEvents[0].TurnoutId;
        }

        // Clear events
        _receivedMessages.Clear();

        // Act - Send turnout command
        var result = await _client.SendTurnoutCmdAsync(turnoutId, true);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Sending turnout command should succeed");

        // Wait for response
        await Task.Delay(1000);

        // Should have received a message about the command
        Assert.That(_receivedMessages, Has.Count.GreaterThan(0), "Should receive message about turnout command");
    }

    [Test]
    public async Task SendRouteCmd_ShouldSendCommandToWiThrottleServer() {
        // Arrange - Connect and wait for initial events
        await _client.ConnectAsync();
        await Task.Delay(2000);

        // Get a route ID from received events if available
        string routeId = "IR1"; // Default fallback
        if (_receivedRouteEvents.Count > 0) {
            routeId = _receivedRouteEvents[0].RouteId;
        }

        // Clear events
        _receivedMessages.Clear();

        // Act - Send route command
        var result = await _client.SendRouteCmdAsync(routeId, true);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Sending route command should succeed");

        // Wait for response
        await Task.Delay(1000);

        // Should have received a message about the command
        Assert.That(_receivedMessages, Has.Count.GreaterThan(0), "Should receive message about route command");
    }

    [Test]
    public async Task SendSignalCmd_ShouldReturnFailure() {
        // Arrange
        await _client.ConnectAsync();

        // Act
        var result = await _client.SendSignalCmdAsync("IH1", SignalAspectEnum.Red);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "Sending signal command should fail");
        Assert.That(result.Error?.Message, Is.EqualTo("Withrottle does not support signal commands."),
                    "Error message should indicate WiThrottle doesn't support signals");
    }

    [Test]
    public async Task Disconnect_ShouldCloseConnectionToWiThrottleServer() {
        // Arrange
        await _client.ConnectAsync();

        // Act
        var result = await _client.DisconnectAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Disconnect should succeed");
        Assert.That(_client.IsConnected, Is.False, "Client should be disconnected");
    }

    [Test]
    public async Task EventHandling_ShouldReceiveEventsFromWiThrottleServer() {
        // Arrange & Act
        await _client.ConnectAsync();

        // Wait for initial events
        await Task.Delay(5000);

        // Assert - We should have received some events
        // Note: This test may be flaky if the WiThrottle server doesn't have any objects defined
        Assert.That(_receivedMessages, Is.Not.Empty, "Should receive messages");

        // Log what we received for debugging
        Console.WriteLine($"Received {_receivedTurnoutEvents.Count} turnout events");
        Console.WriteLine($"Received {_receivedRouteEvents.Count} route events");
        Console.WriteLine($"Received {_receivedMessages.Count} messages");
    }
}