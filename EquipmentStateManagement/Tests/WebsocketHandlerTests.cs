using EquipmentStateManagement.Services;
using NUnit.Framework;
using System.Net;
using System.Text;

namespace EquipmentStateManagement.Tests
    {
    [TestFixture]
    public class WebSocketHandlerTests
        {
        private WebSocketSharp.WebSocket _webSocketClient;
        private string _receivedMessage;
        private HttpListener _mockWebSocketServer;

        [SetUp]
        public void Setup()
            {
            StartMockWebSocketServer();
            _webSocketClient = new WebSocketSharp.WebSocket("ws://localhost:5000/ws");

            _webSocketClient.OnMessage += (sender, e) =>
            {
                _receivedMessage = e.Data;
            };

            _webSocketClient.Connect();
            }

        [Test]
        public async Task WebSocketServer_ShouldBroadcastMessage_OnStateChange()
            {
            _receivedMessage = null;

            var newState = "yellow";
            var equipmentId = "equipment-id";
            string equipmentStateUpdateMessage = $"{{\"id\":\"{equipmentId}\", \"name\":\"Equipment 1\", \"state\":\"{newState}\"}}";

            await SendMessageToWebSocketServer(equipmentStateUpdateMessage);

            await WaitForWebSocketMessage(5000);

            Assert.IsNotNull(_receivedMessage, "No message was received from the WebSocket server.");
            Assert.IsTrue(_receivedMessage.Contains(newState), "The equipment state update was not broadcasted correctly.");
            }

        private void StartMockWebSocketServer()
            {
            _mockWebSocketServer = new HttpListener();
            _mockWebSocketServer.Prefixes.Add("http://localhost:5000/ws/");
            _mockWebSocketServer.Start();

            Task.Run(async () =>
            {
                var context = await _mockWebSocketServer.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                    {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    var webSocket = webSocketContext.WebSocket;

                    await WebSocketHandler.HandleWebSocketAsync(webSocket);
                    }
            });
            }

        private async Task SendMessageToWebSocketServer(string message)
            {
            var buffer = Encoding.UTF8.GetBytes(message);
            _webSocketClient.Send(buffer);
            await Task.Delay(100);
            }

        private async Task WaitForWebSocketMessage(int timeoutMs)
            {
            int elapsed = 0;
            int delayStep = 100;
            while (_receivedMessage == null && elapsed < timeoutMs)
                {
                await Task.Delay(delayStep);
                elapsed += delayStep;
                }
            }

        [TearDown]
        public void TearDown()
            {
            if (_webSocketClient.IsAlive)
                {
                _webSocketClient.Close();
                }

            _mockWebSocketServer.Stop();
            }
        }
    }
