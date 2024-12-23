using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text;
using Castle.Components.DictionaryAdapter.Xml;
using DCCJmriClient;
using Moq;
using Moq.Protected;

namespace DCCJMRIClient.Tests
{
    [TestFixture]
    public class JmriClientTestsMocked
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<IWebSocket> _webSocketMock;
        private JmriClient _client;
        private const string JmriUrl = "http://test-jmri";

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _webSocketMock = new Mock<IWebSocket>();

            // Initialize JmriClient with injection
            _client = new JmriClient(JmriUrl)
            {
                HttpClientFactory = () => new HttpClient(_httpMessageHandlerMock.Object),
                WebSocketFactory = () => _webSocketMock.Object
            };
        }

        [Test]
        public async Task InitializeAsync_ShouldFetchInitialDataAndRaiseEvents()
        {
            // Arrange
            var turnoutData = "[ { \"name\": \"Turnout1\", \"dccAddress\": 1, \"state\": \"CLOSED\", \"locked\": false } ]";
            var routeData = "[ { \"name\": \"Route1\", \"state\": \"ACTIVE\", \"turnouts\": [] } ]";
            var occupancyData = "[ { \"name\": \"Block1\", \"occupied\": true, \"trainId\": \"Train1\" } ]";

            MockHttpResponse("/json/turnouts", turnoutData);
            MockHttpResponse("/json/routes", routeData);
            MockHttpResponse("/json/blocks", occupancyData);

            var turnoutEventRaised = false;
            var routeEventRaised = false;
            var occupancyEventRaised = false;

            _client.TurnoutChanged += (_, args) => { turnoutEventRaised = args.Identifier == "Turnout1"; };
            _client.RouteChanged += (_, args) => { routeEventRaised = args.Identifier == "Route1"; };
            _client.OccupancyChanged += (_, args) => { occupancyEventRaised = args.Identifier == "Block1"; };

            // Act
            try {
                await _client.InitializeAsync();
            } catch (Exception ex) {
                Assert.Fail($"Could not connect to service: {ex.Message}");
            } 

            // Assert
            Assert.That(turnoutEventRaised, Is.True, "Turnout event was not raised.");
            Assert.That(routeEventRaised, Is.True, "Route event was not raised.");
            Assert.That(occupancyEventRaised, Is.True, "Occupancy event was not raised.");
        }

        [Test]
        public async Task StartMonitoringAsync_ShouldConnectToWebSocket()
        {
            // Arrange
            _webSocketMock.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

            // Act
            await _client.StartMonitoringAsync();

            // Assert
            _webSocketMock.Verify(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendTurnoutCommandAsync_ShouldSendCorrectMessage()
        {
            // Arrange
            var sentBytes = Array.Empty<byte>();
            _webSocketMock
                .Setup(ws => ws.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>()))
                .Returns((ReadOnlyMemory<byte> buffer, WebSocketMessageType _, bool _, CancellationToken _) =>
                {
                    sentBytes = buffer.ToArray();
                    return Task.CompletedTask;
                });

            // Act
            await _client.SendTurnoutCommandAsync("Turnout1", true);

            // Assert
            var sentMessage = Encoding.UTF8.GetString(sentBytes);
            Assert.That(sentMessage, Is.EqualTo("{\"type\":\"turnout\",\"identifier\":\"Turnout1\",\"state\":\"THROWN\"}"), "Turnout command was incorrect.");
        }

        // Helper method
        private void MockHttpResponse(string endpoint, string responseData)
        {
            _httpMessageHandlerMock
                .Protected() // Access protected members of HttpMessageHandler
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith(endpoint)), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(responseData)
                });
        }
    }
}