using EquipmentStateManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace EquipmentStateManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private static List<Equipment> _equipment = new List<Equipment>
        {
            new Equipment { Id = Guid.NewGuid(), Name = "Equipment 1", State = "green" },
            new Equipment { Id = Guid.NewGuid(), Name = "Equipment 2", State = "yellow" }
        };

        [HttpGet]
        public IActionResult GetEquipment()
        {
            return Ok(_equipment);
        }

        [HttpPost("{id}/state")]
        public IActionResult UpdateState(Guid id, [FromBody] string newState)
        {
            var equipment = _equipment.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return NotFound();

            equipment.State = newState;
            return Ok(new { message = "State updated" });
        }
    }
}
