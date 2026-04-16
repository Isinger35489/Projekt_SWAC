using StackExchange.Redis;
namespace SIMS.API;

public class RedisSessionService
{
    private readonly IConnectionMultiplexer _redis;
    public RedisSessionService(IConnectionMultiplexer redis) => _redis = redis;

/*
VULNERABILITY: Missing Session Expiry
DESCRIPTION: Sessions werden ohne Ablaufdatum gesetzt und laufen nie ab. Eine gestohlene Session ID ist damit dauerhaft gültig 
    obwohl SessionExpirationMinutes: 60 in der appsettings.json konfiguriert ist, wird der Konfigurationswert ignoriert.
MITIGATION: TimeSpan als Parameter übergeben und beim StringSet als Expiry setzen, z.B: db.StringSet(key, value, TimeSpan.FromMinutes(60))

VULNERABILITY: Missing Key Validation
DESCRIPTION: key wird ohne jede Prüfung direkt an Redis übergeben. Ein Angreifer kann beliebige Keys setzen und lesen und zwar auch interne Keys die nicht für externe Zugriffe gedacht sind.
MITIGATION: Key-Format validieren und nur erlaubte Prefixes zulassen, z.B. nur Keys die mit "session:" beginnen akzeptieren.
*/
    public void SetSession(string key, string value)
    {
        var db = _redis.GetDatabase();

/*
VULNERABILITY: Missing Input Validation 
DESCRIPTION: Weder key noch value werden auf Länge oder Inhalt geprüft. Ein Angreifer kann extrem lange Strings einschleusen und den Redis-Speicher 
damit gezielt zum Absturz bringen.
MITIGATION: Maximale Länge für key und value definieren und ungültige Eingaben mit einer Exception ablehnen.
*/     
        db.StringSet(key, value);
    }
    public string GetSession(string key)
    {
        var db = _redis.GetDatabase();
        return db.StringGet(key);
    }
}
