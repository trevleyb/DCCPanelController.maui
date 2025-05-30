using DCCClients.Jmri;
using DCCClients.Jmri.JMRI;
using DCCCommon.Events;

namespace DCCClients.Test;

[TestFixture]
[Category("Integration")]
public class JmriClientIntegrationTests {
    [OneTimeSetUp]
    public void OneTimeSetUp() {
        var jmriUrl = Environment.GetEnvironmentVariable("JMRI_SERVER_URL");
        if (string.IsNullOrEmpty(jmriUrl)) jmriUrl = "http://localhost:12080";

        _settings = new JmriSettings {
            Name = "ContinuousTest"
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
        _client.TurnoutMsgReceived += ClientOnTurnoutMsgReceived;
        _client.RouteMsgReceived += ClientOnRouteMsgReceived;
        _client.OccupancyMsgReceived += ClientOnOccupancyMsgReceived;
        _client.SignalMsgReceived += ClientOnSignalMsgReceived;
        _client.MessageReceived += ClientOnMessageReceived;
    }

    private DccJmriClient _client = null!;
    private JmriSettings _settings = null!;

    // Event tracking variables
    private List<DccTurnoutArgs> _receivedTurnoutEvents = new();
    private List<DccRouteArgs> _receivedRouteEvents = new();
    private List<DccOccupancyArgs> _receivedOccupancyEvents = new();
    private List<DccSignalArgs> _receivedSignalEvents = new();
    private List<DccMessageArgs> _receivedMessages = new();

    private void ClientOnMessageReceived(object? sender, DccMessageArgs args) {
        _receivedMessages.Add(args);
        Console.WriteLine($"Message=> {args.MessageType}={args.Message}");
    }

    private void ClientOnSignalMsgReceived(object? sender, DccSignalArgs args) {
        _receivedSignalEvents.Add(args);
        Console.WriteLine($"Signal=> {args.SignalId}={args.Aspect}");
    }

    private void ClientOnOccupancyMsgReceived(object? sender, DccOccupancyArgs args) {
        _receivedOccupancyEvents.Add(args);
        Console.WriteLine($"Occupancy=>{args.DccAddress}@{args.BlockId} IsFree={args.IsFree} Occupied={args.IsOccupied}");
    }

    private void ClientOnRouteMsgReceived(object? sender, DccRouteArgs args) {
        _receivedRouteEvents.Add(args);
        Console.WriteLine($"Route=>{args.DccAddress}@{args.RouteId} IsActive={args.IsActive} IsInactive={args.IsInActive}");
    }

    private void ClientOnTurnoutMsgReceived(object? sender, DccTurnoutArgs args) {
        _receivedTurnoutEvents.Add(args);
        Console.WriteLine($"Turnout=>{args.DccAddress}@{args.TurnoutId} Thrown={args.IsThrown} Closed={args.IsClosed} Straight={args.IsStraight} Diverging={args.IsDiverging}");
    }

    [Test]
    public async Task ContinuousRunning() {
        // Act
        var result = await _client.ConnectAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Connection should succeed");
        Assert.That(_client.IsConnected, Is.True, "Client should be connected");

        // Wait a bit to receive initial events

        var startTime = DateTime.Now;
        var endTime = startTime.AddMinutes(0.5);
        while (DateTime.Now < endTime) {
            var elapsed = DateTime.Now - startTime;
            Console.WriteLine($"Waiting for events: {elapsed.Minutes:00}:{elapsed.Seconds:00} {_receivedMessages.Count} messages, {_receivedTurnoutEvents.Count} turnouts, {_receivedRouteEvents.Count} routes, {_receivedOccupancyEvents.Count} occupancies, {_receivedSignalEvents.Count} signals");
            await Task.Delay(5000);
        }

        var disconnected = _client.DisconnectAsync();
        Assert.That(_client.IsConnected, Is.False, "Client should be disconnected");
    }
}