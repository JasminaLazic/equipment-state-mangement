using EquipmentStateManagement.Models;
using EquipmentStateManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentStateManagement.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
        {
        private readonly RedisService _redisService;
        private readonly SQLiteService _sqliteService;

        public EquipmentController(RedisService redisService, SQLiteService sqliteService)
            {
            _redisService = redisService;
            _sqliteService = sqliteService;
            }

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
        public async Task<IActionResult> UpdateState(Guid id, [FromBody] string newState)
            {
            var equipment = _equipment.FirstOrDefault(e => e.Id == id);

            if (equipment == null)
                return new NotFoundObjectResult(new { message = "Equipment not found" });

            _redisService.SetEquipmentState(equipment.Id.ToString(), newState);

            equipment.State = newState;

            await _sqliteService.InsertEquipmentHistory(equipment);

            return Ok(new
                {
                message = "State updated",
                updatedEquipment = equipment
                });
            }
        }
    }
