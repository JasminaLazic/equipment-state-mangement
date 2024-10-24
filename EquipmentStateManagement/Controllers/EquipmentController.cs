using EquipmentStateManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentStateManagement.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
        {
        private readonly IRedisService _redisService;
        private readonly ISQLiteService _sqliteService;
        private readonly IWebSocketHandler _webSocketHandler;
        private readonly IEquipmentService _equipmentService;

        public EquipmentController(
            IRedisService redisService,
            ISQLiteService sqliteService,
            IWebSocketHandler webSocketHandler,
            IEquipmentService equipmentService)
            {
            _redisService = redisService;
            _sqliteService = sqliteService;
            _webSocketHandler = webSocketHandler;
            _equipmentService = equipmentService;
            }

        [HttpGet]
        public IActionResult GetEquipment()
            {
            var equipment = _equipmentService.GetEquipmentList();
            return Ok(equipment);
            }

        [HttpPost("{id}/state")]
        public async Task<IActionResult> UpdateState(Guid id, [FromBody] string newState)
            {
            var equipment = _equipmentService.GetEquipmentById(id);

            if (equipment == null)
                return NotFound(new { message = "Equipment not found" });

            _redisService.SetEquipmentState(equipment.Id.ToString(), newState);
            _equipmentService.UpdateEquipmentState(id, newState);

            await _sqliteService.InsertEquipmentHistory(equipment);

            string message = $"{{\"id\":\"{equipment.Id}\", \"name\":\"{equipment.Name}\", \"state\":\"{newState}\"}}";
            await _webSocketHandler.BroadcastMessageAsync(message);

            return Ok(new
                {
                message = "State updated",
                updatedEquipment = equipment
                });
            }
        }
    }
