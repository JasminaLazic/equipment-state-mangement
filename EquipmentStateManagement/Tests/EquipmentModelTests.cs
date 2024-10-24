using EquipmentStateManagement.Models;
using NUnit.Framework;

namespace EquipmentStateManagement.Tests
    {
    [TestFixture]
    public class EquipmentModelTests
        {
        [Test]
        public void ShouldCreateEquipmentWithIdNameAndState()
            {
            var equipment = new Equipment
                {
                Id = Guid.NewGuid(),
                Name = "Equipment 1",
                State = "green"
                };

            Assert.AreEqual("Equipment 1", equipment.Name);
            Assert.AreEqual("green", equipment.State);
            }
        }
    }
