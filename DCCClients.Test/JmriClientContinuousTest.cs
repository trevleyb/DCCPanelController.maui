using DCCClients.Common;
using DCCClients.Events;
using DCCClients.Jmri;
using DCCClients.Jmri.JMRI;

namespace DCCClients.Test;

[TestFixture]
[Category("Integration")]
public class JmriClientContinuousTest {
    private DccJmriClient _client = null!;
    private JmriSettings _settings = null!;

    // Event tracking variables
    private List<DccTurnoutArgs> _receivedTurnoutEvents = new();
    private List<DccRouteArgs> _receivedRouteEvents = new();
    private List<DccOccupancyArgs> _receivedOccupancyEvents = new();
    private List<DccSignalArgs> _receivedSignalEvents = new();
    private List<DccMessageArgs> _receivedMessages = new();

    [OneTimeSetUp]
    public void OneTimeSetUp() {
        var jmriUrl = Environment.GetEnvironmentVariable("JMRI_SERVER_URL");
        if (string.IsNullOrEmpty(jmriUrl)) jmriUrl = "http://localhost:12080";

        _settings = new JmriSettings {
            JmriServerUrl = jmriUrl,
            Name = "IntegrationTest"
        };
    }

    [SetUp]
    public void Setup() {
        _client = new DccJmriClient(_settings);

        // Clear event tracking
        _receivedTurnoutEvents = new List<DccTurnoutArgs>();
        _receivedRouteEvents = new List<DccRouteArgs>();
        _receivedOccupancyEvents = new List<DccOccupancyArgs>();
        _receivedSignalEvents = new List<DccSignalArgs>();
        _receivedMessages = new List<DccMessageArgs>();

        // Subscribe to events
        _client.TurnoutMsgReceived += (sender, args) => _receivedTurnoutEvents.Add(args);
        _client.RouteMsgReceived += (sender, args) => _receivedRouteEvents.Add(args);
        _client.OccupancyMsgReceived += (sender, args) => _receivedOccupancyEvents.Add(args);
        _client.SignalMsgReceived += (sender, args) => _receivedSignalEvents.Add(args);
        _client.MessageReceived += (sender, args) => _receivedMessages.Add(args);
    }

    [TearDown]
    public void TearDown() {
        _client.Disconnect();
    }

    [Test]
    public async Task ConnectAsync_ShouldConnectToJmriServer() {
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
        _client.Disconnect();

        // Clear events
        _receivedMessages.Clear();

        // Act
        var result = await _client.ReconnectAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Reconnection should succeed");
        Assert.That(_client.IsConnected, Is.True, "Client should be connected");

        // Wait a bit to receive initial events
        await Task.Delay(2000);
    }

    [Test]
    public async Task SendTurnoutCmd_ShouldSendCommandToJmriServer() {
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
        var result = _client.SendTurnoutCmd(turnoutId, true);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Sending turnout command should succeed");

        // Wait for response
        await Task.Delay(1000);

        // Should have received a message about the command
        Assert.That(_receivedMessages, Has.Count.GreaterThan(0), "Should receive message about turnout command");
        Assert.That(_receivedMessages.Any(m => m.MessageType == "Turnout"), Is.True, "Should receive turnout message");
    }

    [Test]
    public async Task SendRouteCmd_ShouldSendCommandToJmriServer() {
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
        var result = _client.SendRouteCmd(routeId, true);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Sending route command should succeed");

        // Wait for response
        await Task.Delay(1000);

        // Should have received a message about the command
        Assert.That(_receivedMessages, Has.Count.GreaterThan(0), "Should receive message about route command");
        Assert.That(_receivedMessages.Any(m => m.MessageType == "Route"), Is.True, "Should receive route message");
    }

    [Test]
    public async Task SendSignalCmd_ShouldSendCommandToJmriServer() {
        // Arrange - Connect and wait for initial events
        await _client.ConnectAsync();
        await Task.Delay(2000);

        // Get a signal ID from received events if available
        string signalId = "IH1"; // Default fallback
        if (_receivedSignalEvents.Count > 0) {
            signalId = _receivedSignalEvents[0].SignalId;
        }

        // Clear events
        _receivedMessages.Clear();

        // Act - Send signal command
        var result = _client.SendSignalCmd(signalId, SignalAspectEnum.Red);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Sending signal command should succeed");

        // Wait for response
        await Task.Delay(1000);

        // Should have received a message about the command
        Assert.That(_receivedMessages, Has.Count.GreaterThan(0), "Should receive message about signal command");
        Assert.That(_receivedMessages.Any(m => m.MessageType == "Signal"), Is.True, "Should receive signal message");
    }

    [Test]
    public async Task Disconnect_ShouldCloseConnectionToJmriServer() {
        // Arrange
        await _client.ConnectAsync();

        // Act
        var result = _client.Disconnect();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Disconnect should succeed");
        Assert.That(_client.IsConnected, Is.False, "Client should be disconnected");
    }

    [Test]
    public async Task EventHandling_ShouldReceiveEventsFromJmriServer() {
        // Arrange & Act
        await _client.ConnectAsync();

        // Wait for initial events
        await Task.Delay(5000);

        // Assert - We should have received some events
        // Note: This test may be flaky if the JMRI server doesn't have any objects defined
        Assert.That(_receivedMessages, Is.Not.Empty, "Should receive messages");

        // Log what we received for debugging
        Console.WriteLine($"Received {_receivedTurnoutEvents.Count} turnout events");
        Console.WriteLine($"Received {_receivedRouteEvents.Count} route events");
        Console.WriteLine($"Received {_receivedOccupancyEvents.Count} occupancy events");
        Console.WriteLine($"Received {_receivedSignalEvents.Count} signal events");
        Console.WriteLine($"Received {_receivedMessages.Count} messages");
    }
}