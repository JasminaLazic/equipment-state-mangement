using EquipmentStateManagement.Controllers;
using EquipmentStateManagement.Models;
using EquipmentStateManagement.Services;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System.Net;
using System.Reflection;
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
        private Mock<IConnectionMultiplexer> _mockRedisConnection;
        private Mock<IDatabase> _mockRedisDatabase;
        private RedisService _redisService;
        private SQLiteService _sqliteService;

        [SetUp]
        public void Setup()
            {
            _mockRedisConnection = new Mock<IConnectionMultiplexer>();
            _mockRedisDatabase = new Mock<IDatabase>();
            _mockRedisConnection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                                .Returns(_mockRedisDatabase.Object);
            _mockRedisDatabase.Setup(db => db.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), null, When.Always, CommandFlags.None))
                              .Returns(true);
            _mockRedisDatabase.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                              .Returns((RedisValue)"yellow");

            _redisService = new RedisService(_mockRedisConnection.Object);
            _sqliteService = new SQLiteService("DataSource=:memory:");
            _equipmentController = new EquipmentController(_redisService, _sqliteService);

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

            var equipmentList = new List<Equipment> { new Equipment { Id = equipmentId, Name = "Equipment 1", State = "green" } };
            typeof(EquipmentController)
                .GetField("_equipment", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, equipmentList);

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

                    await WebSocketHandler.HandleWebSocketAsync(webSocket);
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
