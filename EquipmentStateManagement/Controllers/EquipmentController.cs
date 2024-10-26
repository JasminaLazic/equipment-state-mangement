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

        [HttpGet("/Equipment")]
        public async Task<IActionResult> GetEquipment()
            {
            var equipment = await _equipmentService.GetEquipmentListAsync();
            return Ok(equipment);
            }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateState(Guid id, [FromBody] string newState)
            {
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);

            if (equipment == null)
                return NotFound(new { message = "Equipment not found" });

            _redisService.SetEquipmentState(equipment.Id.ToString(), newState);
            _equipmentService.UpdateEquipmentStateAsync(id, newState);

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
