using EquipmentStateManagement.Models;

namespace EquipmentStateManagement.Services
    {
    public interface IEquipmentService
        {
        List<Equipment> GetEquipmentList();
        Equipment GetEquipmentById(Guid id);
        void UpdateEquipmentState(Guid id, string newState);
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

        public List<Equipment> GetEquipmentList() => _equipment;

        public Equipment GetEquipmentById(Guid id) => _equipment.FirstOrDefault(e => e.Id == id);

        public void UpdateEquipmentState(Guid id, string newState)
            {
            var equipment = GetEquipmentById(id);
            if (equipment != null)
                {
                equipment.State = newState;
                }
            }
        }
    }
