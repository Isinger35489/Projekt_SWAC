using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIMS.Core.Classes;
using SIMS.API.Services;

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

        /* Test-Telegram
        [HttpPost("test-telegram-only")]
        public async Task<IActionResult> TestTelegramOnly()
        {
            var dummy = new Incident
            {
                Id = 9999,
                System = "TEST-SYSTEM",
                Severity = "Low",
                Status = "Open",
                Description = "Dies ist ein Test-Alert",
                CreatedAt = DateTime.Now
            };

            await _telegramAlerter.SendIncidentCreatedAsync(dummy);

            return Ok("Telegram-Alert wurde gesendet (oder Fehler in Console/Error ansehen).");
        }
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

        [HttpPut("{id}")]
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
