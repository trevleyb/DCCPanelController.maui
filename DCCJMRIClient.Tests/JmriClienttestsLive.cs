using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using DCCJmriClient;
using DCCJmriClient.EventArgs;

namespace DCCJMRIClient.Tests;

public class JmriClienttestsLive {
    
    [TestFixture]
    public class JmriClientIntegrationTests {
        private const string JmriUrl = "http://localhost:12080";
        private JmriClient _client;

        [SetUp]
        public void SetUp() {
            // Set up a new JmriClient for each test
            _client = new JmriClient(JmriUrl);
        }

        [TearDown]
        public async Task TearDown() {
            // Clean up and stop the client after each test
            if (_client != null)
                await _client.StopAsync();
        }

        [Test]
        public async Task InitializeAsync_ShouldFetchInitialDataAndRaiseEvents() {
            // Arrange
            var turnoutEvents = new List<TurnoutEventArgs>();
            var routeEvents = new List<RouteEventArgs>();
            var occupancyEvents = new List<OccupancyEventArgs>();

            _client.TurnoutChanged += (_, e) => turnoutEvents.Add(e);
            _client.RouteChanged += (_, e) => routeEvents.Add(e);
            _client.OccupancyChanged += (_, e) => occupancyEvents.Add(e);

            // Act
            await _client.InitializeAsync();

            // Assert (Validate data was fetched and events were raised)
            Assert.That(turnoutEvents.Count, Is.GreaterThan(0), "Turnout data was not fetched or no events were raised.");
            Assert.That(routeEvents.Count, Is.GreaterThan(0), "Route data was not fetched or no events were raised.");
            Assert.That(occupancyEvents.Count, Is.GreaterThan(0), "Occupancy data was not fetched or no events were raised.");
        
            // Optional: Dump the data for manual inspection
            Console.WriteLine("Turnouts:");
            turnoutEvents.ForEach(evt => Console.WriteLine(JsonSerializer.Serialize(evt)));

            Console.WriteLine("Routes:");
            routeEvents.ForEach(evt => Console.WriteLine(JsonSerializer.Serialize(evt)));

            Console.WriteLine("Occupancy:");
            occupancyEvents.ForEach(evt => Console.WriteLine(JsonSerializer.Serialize(evt)));
        }

        [Test]
        public async Task SendTurnoutCommandAsync_ShouldChangeTurnoutState() {
            // Arrange
            var turnoutState = string.Empty;
            _client.TurnoutChanged += (_, e) => {
                if (e.Identifier == "Turnout1") turnoutState = e.State;
            };

            // Act
            await _client.InitializeAsync(); // Ensure the client is initialized
            await _client.SendTurnoutCommandAsync("Turnout1", true); // Send a command to set "Turnout1" to "THROWN"
            await Task.Delay(2000); // Allow time for state to propagate

            // Assert
            Assert.That(turnoutState, Is.EqualTo("THROWN"), "Turnout state was not updated correctly.");
        }

        [Test]
        public async Task SendRouteCommandAsync_ShouldChangeRouteState() {
            // Arrange
            var routeState = string.Empty;
            _client.RouteChanged += (_, e) => {
                if (e.Identifier == "Route1") routeState = e.State;
            };

            // Act
            await _client.InitializeAsync(); // Ensure the client is initialized
            await _client.SendRouteCommandAsync("Route1"); // Send a command to activate "Route1"
            await Task.Delay(2000); // Allow time for state to propagate

            // Assert
            Assert.That(routeState, Is.EqualTo("ACTIVE"), "Route state was not updated correctly.");
        }

        [Test]
        public async Task StartMonitoringAsync_ShouldReceiveWebSocketUpdates() {
            // Arrange
            var receivedTurnoutUpdates = new List<TurnoutEventArgs>();
            var receivedRouteUpdates = new List<RouteEventArgs>();
            var receivedOccupancyUpdates = new List<OccupancyEventArgs>();

            _client.TurnoutChanged += (_, e) => receivedTurnoutUpdates.Add(e);
            _client.RouteChanged += (_, e) => receivedRouteUpdates.Add(e);
            _client.OccupancyChanged += (_, e) => receivedOccupancyUpdates.Add(e);

            // Act
            await _client.InitializeAsync(); // Initialize to load initial data
            await _client.StartMonitoringAsync(); // Start monitoring the JSON server
            await Task.Delay(10000); // Allow sufficient time to receive updates

            // Assert
            Assert.That(receivedTurnoutUpdates.Count, Is.GreaterThan(0), "No turnout updates were received.");
            Assert.That(receivedRouteUpdates.Count, Is.GreaterThan(0), "No route updates were received.");
            Assert.That(receivedOccupancyUpdates.Count, Is.GreaterThan(0), "No occupancy updates were received.");
            
            // Optional: Log received data
            Console.WriteLine($"Received {receivedTurnoutUpdates.Count} turnout updates.");
            Console.WriteLine($"Received {receivedRouteUpdates.Count} route updates.");
            Console.WriteLine($"Received {receivedOccupancyUpdates.Count} occupancy updates.");
        }

        [Test]
        public async Task StopAsync_ShouldGracefullyDisconnectAndStopMonitoring() {
            // Arrange
            await _client.InitializeAsync();
            await _client.StartMonitoringAsync();

            // Act
            await _client.StopAsync(); // Stop the client
            await Task.Delay(2000); // Give time for any asynchronous resources to clean up

            // Assert
            Assert.Pass("Client stopped without exceptions.");
        }
    }
}