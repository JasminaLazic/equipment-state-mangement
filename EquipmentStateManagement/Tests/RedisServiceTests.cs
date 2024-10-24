using EquipmentStateManagement.Services;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;

namespace EquipmentStateManagement.Tests
    {
    [TestFixture]
    public class RedisServiceTests
        {
        private RedisService _redisService;
        private Mock<IConnectionMultiplexer> _mockRedis;
        private Mock<IDatabase> _dbMock;
        [SetUp]
        public void Setup()
            {
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _dbMock = new Mock<IDatabase>();

            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_dbMock.Object);

            _dbMock.Setup(db => db.StringSet(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>())).Returns(true);

            _redisService = new RedisService(_mockRedis.Object);
            }

        [Test]
        public void ShouldSetAndGetEquipmentState()
            {
            _dbMock.Setup(db => db.StringSet(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            )).Returns(true);

            _redisService.SetEquipmentState("Equipment1", "green");

            _dbMock.Verify(db => db.StringSet(
                "Equipment1",
                "green",
                null,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ), Times.Once);
            }

        [Test]
        public void ShouldRetrieveEquipmentState()
            {
            _dbMock.Setup(db => db.StringGet("Equipment1", It.IsAny<CommandFlags>())).Returns("green");

            var state = _redisService.GetEquipmentState("Equipment1");

            Assert.AreEqual("green", state);
            _dbMock.Verify(db => db.StringGet("Equipment1", CommandFlags.None), Times.Once);
            }
        }
    }
