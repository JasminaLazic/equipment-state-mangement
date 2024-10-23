using EquipmentStateManagement.Controllers;
using EquipmentStateManagement.Models;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentStateManagement.Tests
{
    public class EquipmentControllerTests
    {
        private EquipmentController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new EquipmentController();
        }

        [Test]
        public void ShouldReturnEquipmentList()
        {
            var result = _controller.GetEquipment() as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void ShouldUpdateEquipmentState()
        {
            var result = _controller.UpdateState(Guid.NewGuid(), "red") as NotFoundResult;
            Assert.AreEqual(404, result.StatusCode);
        }


        [Test]
        public void ShouldUpdateEquipmentState_Success()
        {
            var getResult = _controller.GetEquipment() as OkObjectResult;
            var equipmentList = getResult.Value as List<Equipment>;
            var existingEquipment = equipmentList[0];

            var updateResult = _controller.UpdateState(existingEquipment.Id, "red") as OkObjectResult;

            Assert.IsNotNull(updateResult);
            Assert.AreEqual(200, updateResult.StatusCode);

            var updatedEquipmentList = (_controller.GetEquipment() as OkObjectResult).Value as List<Equipment>;
            var updatedEquipment = updatedEquipmentList.FirstOrDefault(e => e.Id == existingEquipment.Id);

            Assert.IsNotNull(updatedEquipment);
            Assert.AreEqual("red", updatedEquipment.State);
        }
    }
}
