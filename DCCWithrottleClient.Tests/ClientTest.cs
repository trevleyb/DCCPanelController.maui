using System.Diagnostics;
using System.Globalization;
using DCCWithrottleClient.Client;
using DCCWithrottleClient.Client.Commands;
using DCCWithrottleClient.Client.Events;
using DCCWithrottleClient.ServiceHelper;

namespace DCCWithrottleClient.Tests;

[TestFixture]
public class ClientTest {
    [Test]
    public async Task FindServers() {
        var servers = await ServiceFinder.FindServices("withrottle");
        Assert.That(servers.Count, Is.GreaterThanOrEqualTo(1));
    }

    public async Task<ClientInfo?> GetDefaultWiServer() {
        Debug.WriteLine("testing Connection to WiThrottle Server");
        var server = await ServiceFinder.FindServices("withrottle");
        if (server.Count == 0 || server[0]?.ClientInfo != null) return null;
        return server[0]?.ClientInfo;
    }

    [Test]
    public async Task RunConnectionTest() {
        Debug.WriteLine("testing Connection to WiThrottle Server");

        if (await GetDefaultWiServer() is { } clientInfo) {
            var client = new Client.Client(clientInfo);
            client.ConnectionError += ClientOnConnectionError;
            client.ConnectionEvent += ClientOnConnectionEvent;
            client.Connect();

            for (var i = 0; i < 10; i++) {
                Debug.WriteLine($"Waiting... {i}");
                Thread.Sleep(1000);
            }

            client.Disconnect();
        }

        Thread.Sleep(1000);
        Debug.WriteLine("Completed.");
    }

    [Test]
    public async Task SendCommandTests() {
        Debug.WriteLine("testing Connection to WiThrottle Server");

        if (await GetDefaultWiServer() is { } clientInfo) {
            var client = new Client.Client(clientInfo);
            client.ConnectionError += ClientOnConnectionError;
            client.ConnectionEvent += ClientOnConnectionEvent;
            client.Connect();

            // Sleep for 5 seconds to let all messages get processed.
            Debug.WriteLine("Waiting for 5 seconds...");
            Thread.Sleep(5000);

            client.SendMessage(new TurnoutCommand("NT127", TurnoutStateEnum.Closed));
            client.SendMessage(new TurnoutCommand("NT126", TurnoutStateEnum.Thrown));
            client.SendMessage(new TurnoutCommand("NT137", TurnoutStateEnum.Toggle));

            client.SendMessage(new RouteCommand("ROUTE1"));
            client.SendMessage(new RouteCommand("ROUTE2"));
            client.SendMessage(new RouteCommand("ROUTE3"));

            Debug.WriteLine("Waiting for 5 seconds...");
            Thread.Sleep(5000);
            client.Disconnect();
        }

        Thread.Sleep(1000);
        Debug.WriteLine("Completed.");
    }

    [Test]
    public async Task TestFastClock() {
        Debug.WriteLine("testing Connection to WiThrottle Server");

        if (await GetDefaultWiServer() is { } clientInfo) {
            var client = new Client.Client(clientInfo);
            client.ConnectionError += ClientOnConnectionError;
            client.ConnectionEvent += ClientOnConnectionEvent;
            client.Connect();

            // Sleep for 5 seconds to let all messages get processed.
            Debug.WriteLine("Waiting for 5 seconds...");
            Thread.Sleep(5000);

            client.SendMessage(new FastClockCommand(DateTime.Now, 4));

            // Sleep for 30 seconds so we get the FastClock Messages
            Debug.WriteLine("Waiting for 30 seconds...");
            Thread.Sleep(30000);
            client.Disconnect();
        }

        Thread.Sleep(1000);
        Debug.WriteLine("Completed.");
    }

    private void ClientOnConnectionEvent(IClientEvent clientevent) {
        switch (clientevent) {
        case MessageEvent message:
            Debug.WriteLine($"MESSAGE: {message.Type} => {message.Value}");
            break;

        case RosterEvent roster:
            Debug.WriteLine("ROSTER: Message");
            break;

        case RouteEvent route:
            Debug.WriteLine($"ROUTE:{route.SystemName} : {route.UserName} => {route.State}");
            break;

        case TurnoutEvent turnout:
            Debug.WriteLine($"TURNOUT: {turnout.SystemName} : {turnout.UserName} => {turnout.State}");
            break;

        case FastClockEvent clock:
            Debug.WriteLine($"CLOCK: {clock.Time.ToString(CultureInfo.InvariantCulture)}");
            break;

        default:
            Debug.WriteLine($"UNKNOWN: {clientevent.ToString()}");
            break;
        }
    }

    private void ClientOnConnectionError(string obj) {
        Debug.WriteLine("ERROR: " + obj);
    }
}