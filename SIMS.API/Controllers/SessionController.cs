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

VULNERABLE Code: gesammter Controller
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

            [HttpGet]
            public IActionResult Get([FromQuery] string key)
            {
                var value = _service.GetSession(key);
                return Ok(value);
            }
        }
 
}
