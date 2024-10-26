using EquipmentStateManagement.Controllers;
using EquipmentStateManagement.Models;
using EquipmentStateManagement.Services;
using Moq;
using NUnit.Framework;
using System.Net;
using WebSocketSharp;

namespace EquipmentStateManagement.Tests
{
    [TestFixture]
    public class WebSocketHandlerTests
    {
        private WebSocket _webSocketClient;
        private string _receivedMessage;
        private HttpListener _mockWebSocketServer;
        private EquipmentController _equipmentController;
        private Mock<IRedisService> _mockRedisService;
        private Mock<ISQLiteService> _mockSQLiteService;
        private Mock<IEquipmentService> _mockEquipmentService;

        [SetUp]
        public void Setup()
        {
            _mockRedisService = new Mock<IRedisService>();
            _mockRedisService.Setup(x => x.SetEquipmentState(It.IsAny<string>(), It.IsAny<string>()))
                             .Verifiable();
            _mockRedisService.Setup(x => x.GetEquipmentState(It.IsAny<string>()))
                             .Returns("yellow");

            _mockSQLiteService = new Mock<ISQLiteService>();
            _mockSQLiteService.Setup(x => x.InsertEquipmentHistory(It.IsAny<Equipment>()))
                              .Returns(Task.CompletedTask);

            _mockEquipmentService = new Mock<IEquipmentService>();
            _mockEquipmentService.Setup(x => x.GetEquipmentByIdAsync(It.IsAny<Guid>()))
                                 .ReturnsAsync(new Equipment { Id = Guid.NewGuid(), Name = "Equipment 1", State = "green" });

            _equipmentController = new EquipmentController(
                _mockRedisService.Object,
                _mockSQLiteService.Object,
                new WebSocketHandler(),
                _mockEquipmentService.Object
            );

            StartMockWebSocketServer();
            _webSocketClient = new WebSocket("ws://localhost:5000/ws");

            _webSocketClient.OnMessage += (sender, e) =>
                {
                    _receivedMessage = e.Data;
                };

            _webSocketClient.Connect();
        }

        [Test]
        public async Task WebSocket_ShouldReceiveMessage_WhenEquipmentStateIsUpdated()
        {
            _receivedMessage = null;

            var equipmentId = Guid.NewGuid();
            var newState = "yellow";

            await _equipmentController.UpdateState(equipmentId, newState);

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

                    await new WebSocketHandler().HandleWebSocketAsync(webSocket);
                }
            });
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
