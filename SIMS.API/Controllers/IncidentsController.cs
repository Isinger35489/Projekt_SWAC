using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Core.Classes;

namespace SIMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly SimsDbContext _context;
        private readonly RedisSessionService _redisService;
        public IncidentsController(SimsDbContext context, RedisSessionService redisService)
        {
            _context = context;
            _redisService = redisService;
        }
       [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> GetAll()
        {
            // In Redis loggen
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

        [HttpPost]
        public async Task<IActionResult> Post(Incident incident)
        {
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);  // Zeigt Validierungsfehler
        //    }

        //    // Setze CVE auf null wenn leer
        //    if (string.IsNullOrWhiteSpace(incident.CVE))
        //    {
        //        incident.CVE = null;
        //    }

            // Setze CreatedAt serverseitig
            incident.CreatedAt = DateTime.Now;

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            _redisService.SetSession($"incident:{incident.Id}:created", DateTime.Now.ToString());
            _redisService.SetSession("last_incident_created", incident.Id.ToString());

            return CreatedAtAction(nameof(Get), new { id = incident.Id }, incident);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Incident incident)
        {
            if (id != incident.Id) return BadRequest();
            
           
            if(!ModelState.IsValid)
    {
                return BadRequest(ModelState);
            }

            //// Wenn bei CVE nichts eingefügt ist es autmoatisch null
            //if (string.IsNullOrWhiteSpace(incident.CVE))
            //{
            //    incident.CVE = null;
            //}


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


