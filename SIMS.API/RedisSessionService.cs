using StackExchange.Redis;
namespace SIMS.API;

public class RedisSessionService
{
    private readonly IConnectionMultiplexer _redis;
    public RedisSessionService(IConnectionMultiplexer redis) => _redis = redis;

    public void SetSession(string key, string value)
    {
        var db = _redis.GetDatabase();
        db.StringSet(key, value);
    }
    public string GetSession(string key)
    {
        var db = _redis.GetDatabase();
        return db.StringGet(key);
    }
}
