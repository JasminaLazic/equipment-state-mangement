using EquipmentStateManagement.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentStateManagement.Services
    {
    public interface IEquipmentService
        {
        Task<List<Equipment>> GetEquipmentListAsync();
        Task<Equipment> GetEquipmentByIdAsync(Guid id);
        Task UpdateEquipmentStateAsync(Guid id, string newState);
        }

    public class EquipmentService : IEquipmentService
        {
        private static readonly List<Equipment> _equipment;

        static EquipmentService()
            {
            _equipment = new List<Equipment>
            {
                new Equipment { Id = Guid.NewGuid(), Name = "Equipment 1", State = "green" },
                new Equipment { Id = Guid.NewGuid(), Name = "Equipment 2", State = "yellow" }
            };
            }

        public async Task<List<Equipment>> GetEquipmentListAsync()
            {
            return await Task.FromResult(_equipment);
            }

        public async Task<Equipment> GetEquipmentByIdAsync(Guid id)
            {
            return await Task.FromResult(_equipment.FirstOrDefault(e => e.Id == id));
            }

        public async Task UpdateEquipmentStateAsync(Guid id, string newState)
            {
            var equipment = await GetEquipmentByIdAsync(id);
            if (equipment != null)
                {
                equipment.State = newState;
                }
            }
        }
    }
