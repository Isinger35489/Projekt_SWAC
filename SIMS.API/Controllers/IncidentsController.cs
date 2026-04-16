using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIMS.Core.Classes;
using SIMS.API.Services;



/*
Schadlevel: KRITISCH​
VULNERABILITY: Broken Access Control ​
DESCRIPTION: Es existiert keine explizite Autorisierung auf Controller oder Endpunkte. 
Dadurch können Benutzer mit Zugriff auf die API z.B. über API-Key Sicherheitskritische Operationen ohne Rollenprüfung durchführen.
MITIGATION: Auf Controller-Ebene eine Authentifizierung setzen.​ Sensible Operationen wie DELETE oder PUT zusätzlich auf bestimmte Rollen (z.B. Administrator) zu beschränken​
*/
//VULNERABLE Code: Controller-Ebene
namespace SIMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly SimsDbContext _context;
        private readonly RedisSessionService _redisService;
        private readonly TelegramAlerter _telegramAlerter;
        private readonly ILogger<IncidentsController> _logger;

        public IncidentsController(
            SimsDbContext context,
            RedisSessionService redisService,
            TelegramAlerter telegramAlerter,
            ILogger<IncidentsController> logger)
        {
            _context = context;
            _redisService = redisService;
            _telegramAlerter = telegramAlerter;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> GetAll()
        {
            _redisService.SetSession("last_access", DateTime.Now.ToString());
            return await _context.Incidents.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Incident>> Get(int id)
        {
            _redisService.SetSession($"incident:{id}:last_viewed", DateTime.Now.ToString());

            var incident = await _context.Incidents.FindAsync(id);
            return incident ?? (ActionResult<Incident>)NotFound();
        }

/* 
VULNERABILITY: Missing Input Sanitization (XXS)​
DESCRIPTION: Textfelder wie Description werden ohne Längenbegrenzung oder Encoding entgegengenommen. 
Schadcode kann über Description in die Datenbank geschrieben und später im Frontend ausgeführt werden.
MITIGATION: DTO mit Validierungsattributen ([MaxLength], [RegularExpression]) verwenden.
    HtmlEncoder.Default.Encode() verwenden. Anti-XSS Library nutzen. Content Security Policy (CSP) implementieren.
VULNERABLE CODE:
*/ 
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Incident incident)
        {
            if (incident == null)
                return BadRequest("Incident darf nicht null sein.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            incident.CreatedAt = DateTime.Now;

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            _redisService.SetSession($"incident:{incident.Id}:created", DateTime.Now.ToString());
            _redisService.SetSession("last_incident_created", incident.Id.ToString());

            // Telegram-Alert soll die API nicht kaputt machen, wenn etwas schiefgeht
            try
            {
                await _telegramAlerter.SendIncidentCreatedAsync(incident);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fehler beim Senden des Telegram-Alerts für Incident {IncidentId}",
                    incident.Id);
            }

            return CreatedAtAction(nameof(Get), new { id = incident.Id }, incident);
        }


/*
VULNERABILITY: Missing Input Sanitization / Over-Posting
DESCRIPTION: Das gesamte Incident-Objekt wird ungefiltert entgegengenommen ohne zu prüfen ob der Eintrag überhaupt existiert. Interne Felder wie Severity oder Status 
können direkt überschrieben werden.
MITIGATION: Nur eine eigene Eingabe-Klasse mit erlaubten Feldern akzeptieren. Mit FindAsync() sicherstellen dass der Eintrag existiert bevor er überschrieben wird. 
 HtmlEncoder.Default.Encode() auf Textfelder anwenden.
*/
        [HttpPut("{id}")]
/* 
VULNERABILITY: Missing Input Sanitization (XXS)​
DESCRIPTION: Textfelder wie Description werden ohne Längenbegrenzung oder Encoding entgegengenommen. 
Schadcode kann über Description in die Datenbank geschrieben und später im Frontend ausgeführt werden.​
MITIGATION: HtmlEncoder.Default.Encode() verwenden. Anti-XSS Library nutzen. Content Security Policy (CSP) implementieren.
VULNERABLE CODE:
*/ 
        public async Task<IActionResult> Put(int id, [FromBody] Incident incident)
        {
            if (incident == null)
                return BadRequest("Incident darf nicht null sein.");

            if (id != incident.Id)
                return BadRequest("ID in URL und Body stimmen nicht überein.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(incident).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _redisService.SetSession($"incident:{id}:last_updated", DateTime.Now.ToString());

            return NoContent();
        }
/*
VULNERABILITY: Insecure Direct Object Reference (IDOR)
DESCRIPTION: Jeder kann durch Angabe einer beliebigen ID fremde Incidents löschen ohne dass geprüft wird ob er dazu berechtigt ist.
MITIGATION: Nach [Authorize] prüfen ob der anfragende User Eigentümer des Eintrags ist oder eine Administrator-Rolle besitzt.
*/
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var toDelete = await _context.Incidents.FindAsync(id);
            if (toDelete == null) return NotFound();

            _context.Incidents.Remove(toDelete);
            await _context.SaveChangesAsync();

            _redisService.SetSession($"incident:{id}:deleted", DateTime.Now.ToString());

            return NoContent();
        }
    }
}
