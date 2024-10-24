using EquipmentStateManagement.Controllers;
using EquipmentStateManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;


namespace EquipmentStateManagement.Tests
{
    [TestFixture]
    public class EquipmentControllerTests
    {
        private EquipmentController _controller;
        private Mock<SQLiteService> _mockSqliteService;

        [SetUp]
        public void Setup()
        {
            _mockSqliteService = new Mock<SQLiteService>("DataSource=:memory:");
            _controller = new EquipmentController(_mockSqliteService.Object);
        }

        [Test]
        public void ShouldReturnEquipmentList()
        {
            var result = _controller.GetEquipment() as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task ShouldUpdateEquipmentState_NotFound()
        {
            var result = await _controller.UpdateState(Guid.NewGuid(), "red") as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);

            var resultValue = result.Value as dynamic;
            Assert.IsNotNull(resultValue);
            Assert.AreEqual("Equipment not found", resultValue.message);
        }


    }
}
