using Microsoft.AspNetCore.Mvc;
namespace SIMS.API.Controllers
{


/*
Level:  KRITISCH​
VULNERABILITY: Session Forgery​
DESCRIPTION: Angreifer kann beliebige Session lesen und da "Set" offen ist, kann ein Angreifer beliebige Sessions anlegen.
Es kann z.B. eine Admin Session ohne Login gestartet werden, mit der ein voller Redis-Dump (Session Store) möglich ist.​
MITIGATION: Authentifizierung und Session-Mangement einbauen. ​
Mindestens Administrator-Rolle sollte erforderlich sein um Sessions zu lesen oder zu schreiben zudem sollte ein​
standardisiertes System wie JWT Bearer Tokens oder ASP.NET Core Identity eingebaut werden.

*/
        [ApiController]
        [Route("api/[controller]")]
        public class SessionController : ControllerBase
        {
            private readonly RedisSessionService _service;

            public SessionController(RedisSessionService service)
            {
                _service = service;
            }


            [HttpPost]
            public IActionResult Set([FromQuery] string key, [FromQuery] string value)
            {
                _service.SetSession(key, value);
                return Ok();
            }
/*
VULNERABILITY: Session Hijacking
DESCRIPTION: key wird ungefiltert entgegengenommen und gibt den kompletten Session-Inhalt zurück. Ein Angreifer kann durch Durchprobieren von Keys alle aktiven Sessions auslesen und übernehmen.
MITIGATION: Endpoint komplett entfernen. Session-Validierung gehört intern in die Anwendung und nicht als öffentlicher Endpoint.
*/
            [HttpGet]

/*
VULNERABILITY: Sensitive Data in URL
DESCRIPTION: key und value werden als Query-Parameter in der URL übertragen und landen damit in Server-Logs, Proxy-Logs und Browser-History. 
        Session-Inhalte wie "1|admin|Administrator" sind damit dauerhaft aus Logs rekonstruierbar.
MITIGATION: Sensible Daten niemals als Query-Parameter übertragen. Stattdessen Request-Body oder Authorization-Header verwenden.
*/
            public IActionResult Get([FromQuery] string key)
            {

/*
VULNERABILITY: Missing Error Handling
DESCRIPTION: Wenn GetSession() null zurückgibt wird Ok(null) zurückgegeben statt einem aussagekräftigen Fehler. Ein Angreifer kann damit systematisch 
        gültige Session-Keys erraten ohne dass das Verhalten auffällig wird.
MITIGATION: Prüfen ob der Wert null ist und NotFound() zurückgeben. Zusätzlich Rate Limiting einbauen um Key-Enumeration zu verhindern.
*/
                var value = _service.GetSession(key);
                return Ok(value);
            }
        }
 
}
