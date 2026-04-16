using Microsoft.AspNetCore.Mvc;
namespace SIMS.API.Controllers
{


/*
Level:  KRITISCH​
VULNERABILITY: Session Forgery​
DESCRIPTION: Angreifer kann beliebige Session lesen und da "Set" offen ist, kann ein Angreifer beliebige Sessions anlegen.
Über den Set-Endpoint können neue Session-Einträge erzeugt oder bestehende manipuliert werden, z. B. eine Admin-Session
ohne Login. Damit ist ein Missbrauch des gesamten Session-Stores möglich.
MITIGATION: Authentifizierung und Session-Mangement einbauen. ​
Mindestens Administrator-Rolle sollte erforderlich sein um Sessions zu lesen oder zu schreiben.
Langfristig ein standardisiertes System wie JWT Bearer Tokens oder ASP.NET Core Identity verwenden.

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
/*
    VULNERABILITY: Session Forgery
    DESCRIPTION: Der Endpoint erlaubt das Schreiben beliebiger Session-Daten über einen
    frei wählbaren Key, ohne Authentifizierung oder Rollenprüfung. 
    Angreifer können damit Sessions anlegen oder manipulieren, z. B. eine Admin-Session ohne Login.
    MITIGATION: Den Endpoint absichern oder vollständig entfernen. 
    Nur autorisierte Administratoren sollten Session-Daten schreiben dürfen.
    */
            [HttpPost]
            public IActionResult Set([FromQuery] string key, [FromQuery] string value)
            {
                _service.SetSession(key, value);
                return Ok();
            }

  /*
VULNERABILITY: Session Hijacking / Broken Access Control / Sensitive Data Exposure
DESCRIPTION: Der Endpoint gibt Session-Inhalte zu einem frei übergebenen Key zurück,
ohne dass eine sichtbare Authentifizierung oder Rechteprüfung vorhanden ist.
Ein Angreifer kann dadurch Session-Daten unautorisiert auslesen und für eine
Übernahme aktiver Sessions missbrauchen.
MITIGATION: Endpoint entfernen oder absichern und Session-Daten nur intern verarbeiten.
Session-Validierung gehört nicht in einen öffentlich erreichbaren Endpoint.
*/
            [HttpGet]

/*
VULNERABILITY: Sensitive Data in URL
DESCRIPTION: Key und Value werden als Query-Parameter in der URL übertragen. 
Solche Werte können in Browser-History, Server-Logs oder anderen Protokollen landen.
Session-Inhalte wie "1|admin|Administrator" sind damit dauerhaft aus Logs rekonstruierbar.
MITIGATION: Sensible Daten nicht über Query-Parameter übertragen. Stattdessen Request-Body oder Authorization-Header verwenden.
*/
            public IActionResult Get([FromQuery] string key)
            {

/*
VULNERABILITY: Missing Error Handling
DESCRIPTION: Wenn GetSession() null zurückgibt, wird Ok(null) zurückgegeben statt eines aussagekräftigen Fehlers. 
Angreifer können damit systematisch gültige Session-Keys ausprobieren, ohne dass dies klar auffällt.
MITIGATION: Prüfen, ob der Wert null ist und NotFound() oder eine passende Fehlerantwort zurückgeben. 
Zusätzlich Rate Limiting einbauen, um Key-Enumeration zu erschweren.
*/
                var value = _service.GetSession(key);
                return Ok(value);
            }
        }
 
}
