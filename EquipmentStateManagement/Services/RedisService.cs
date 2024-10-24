using StackExchange.Redis;

namespace EquipmentStateManagement.Services
    {
    public class RedisService
        {
        private readonly IConnectionMultiplexer _redis;
        public RedisService(IConnectionMultiplexer redis)
            {
            _redis = redis;
            }

        public virtual void SetEquipmentState(string key, string state)
            {
            var db = _redis.GetDatabase();
            db.StringSet(key, state, null, When.Always, CommandFlags.None);
            }


        public string GetEquipmentState(string key)
            {
            var db = _redis.GetDatabase();
            return db.StringGet(key);
            }
        }
    }
