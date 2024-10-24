using EquipmentStateManagement.Controllers;
using EquipmentStateManagement.Models;
using EquipmentStateManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace EquipmentStateManagement.Tests
    {
    [TestFixture]
    public class EquipmentControllerTests
        {
        private ServiceProvider _serviceProvider;
        private Mock<IRedisService> _mockRedisService;
        private Mock<ISQLiteService> _mockSqliteService;
        private Mock<IWebSocketHandler> _mockWebSocketHandler;
        private Mock<IEquipmentService> _mockEquipmentService;

        [SetUp]
        public void Setup()
            {
            var serviceCollection = new ServiceCollection();

            _mockRedisService = new Mock<IRedisService>();
            _mockSqliteService = new Mock<ISQLiteService>();
            _mockWebSocketHandler = new Mock<IWebSocketHandler>();
            _mockEquipmentService = new Mock<IEquipmentService>();

            _mockEquipmentService.Setup(s => s.GetEquipmentList())
                .Returns(new List<Equipment>
                {
            new Equipment { Id = Guid.NewGuid(), Name = "Equipment 1", State = "green" },
            new Equipment { Id = Guid.NewGuid(), Name = "Equipment 2", State = "yellow" }
                });

            serviceCollection.AddSingleton(_mockRedisService.Object);
            serviceCollection.AddSingleton(_mockSqliteService.Object);
            serviceCollection.AddSingleton(_mockWebSocketHandler.Object);
            serviceCollection.AddSingleton(_mockEquipmentService.Object);

            serviceCollection.AddTransient<EquipmentController>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
            }


        [Test]
        public void ShouldReturnEquipmentList()
            {
            var controller = _serviceProvider.GetService<EquipmentController>();

            var result = controller.GetEquipment() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var equipmentList = result.Value as List<Equipment>;
            Assert.IsNotNull(equipmentList);
            Assert.IsTrue(equipmentList.Count > 0, "Equipment list should not be empty.");
            }

        [Test]
        public async Task ShouldUpdateEquipmentState_NotFound()
            {
            var controller = _serviceProvider.GetService<EquipmentController>();

            var result = await controller.UpdateState(Guid.NewGuid(), "red") as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);

            var resultValue = result.Value as dynamic;
            Assert.IsNotNull(resultValue);
            Assert.AreEqual("Equipment not found", resultValue.message);
            }

        [Test]
        public async Task ShouldUpdateEquipmentState_Success()
            {
            var existingEquipment = new Equipment { Id = Guid.NewGuid(), Name = "Equipment 1", State = "green" };

            _mockEquipmentService.Setup(s => s.GetEquipmentById(existingEquipment.Id))
                .Returns(existingEquipment);

            _mockRedisService.Setup(r => r.SetEquipmentState(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            _mockSqliteService.Setup(s => s.InsertEquipmentHistory(It.IsAny<Equipment>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockEquipmentService.Setup(s => s.UpdateEquipmentState(existingEquipment.Id, It.IsAny<string>()))
                .Callback<Guid, string>((id, newState) => existingEquipment.State = newState);

            var controller = _serviceProvider.GetService<EquipmentController>();

            var updateResult = await controller.UpdateState(existingEquipment.Id, "red") as OkObjectResult;

            Assert.IsNotNull(updateResult, "Expected OkObjectResult but got null");
            Assert.AreEqual(200, updateResult.StatusCode, "Expected status code 200 for successful update");

            _mockRedisService.Verify(r => r.SetEquipmentState(existingEquipment.Id.ToString(), "red"), Times.Once);

            _mockSqliteService.Verify(s => s.InsertEquipmentHistory(It.IsAny<Equipment>()), Times.Once);

            Assert.AreEqual("red", existingEquipment.State, "The equipment state should be updated to 'red'");
            }


        [TearDown]
        public void TearDown()
            {
            if (_serviceProvider != null)
                {
                _serviceProvider.Dispose();
                }
            }
        }
    }
