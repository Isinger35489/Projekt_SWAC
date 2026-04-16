using StackExchange.Redis;
namespace SIMS.API;

public class RedisSessionService
{
    private readonly IConnectionMultiplexer _redis;
    public RedisSessionService(IConnectionMultiplexer redis) => _redis = redis;

/*
VULNERABILITY: Missing Session Expiry
DESCRIPTION: Sessions werden ohne Ablaufdatum gesetzt und laufen nie ab. E ine gestohlene Session ID ist damit dauerhaft gültig 
    obwohl SessionExpirationMinutes: 60 in der appsettings.json konfiguriert ist, wird der Konfigurationswert ignoriert.
MITIGATION: TimeSpan als Parameter übergeben und beim StringSet als Expiry setzen, z.B: db.StringSet(key, value, TimeSpan.FromMinutes(60))

VULNERABILITY: Missing Key Validation
DESCRIPTION: key wird ohne jede Prüfung direkt an Redis übergeben. Ein Angreifer kann beliebige Keys setzen und lesen — auch interne Keys die nicht für externe Zugriffe gedacht sind.
MITIGATION: Key-Format validieren und nur erlaubte Prefixes zulassen, z.B. nur Keys die mit "session:" beginnen akzeptieren.
*/
    public void SetSession(string key, string value)
    {
        var db = _redis.GetDatabase();

/*
VULNERABILITY: Missing Input Validation
DESCRIPTION: Weder key noch value werden auf Länge oder Inhalt geprüft. Ein Angreifer kann beliebig lange Strings einschleusen was zu Memory-Problemen im Redis-Store führen kann.
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
