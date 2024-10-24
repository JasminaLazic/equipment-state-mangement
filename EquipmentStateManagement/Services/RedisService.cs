using StackExchange.Redis;

namespace EquipmentStateManagement.Services
    {
    public interface IRedisService
        {
        void SetEquipmentState(string key, string state);
        string GetEquipmentState(string key);
        }

    public class RedisService : IRedisService
        {
        private readonly IConnectionMultiplexer _redis;

        public RedisService(IConnectionMultiplexer redis)
            {
            _redis = redis;
            }

        public void SetEquipmentState(string key, string state)
            {
            var db = _redis.GetDatabase();
            db.StringSet(key, state);
            }

        public string GetEquipmentState(string key)
            {
            var db = _redis.GetDatabase();
            return db.StringGet(key);
            }
        }
    }
