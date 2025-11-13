using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Core;

namespace SIMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly SimsDbContext _context;
        public IncidentsController(SimsDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incident>>> GetAll()
            => await _context.Incidents.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Incident>> Get(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            return incident ?? (ActionResult<Incident>)NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post(Incident incident)
        {
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = incident.Id }, incident);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Incident incident)
        {
            if (id != incident.Id) return BadRequest();

            _context.Entry(incident).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var toDelete = await _context.Incidents.FindAsync(id);
            if (toDelete == null) return NotFound();

            _context.Incidents.Remove(toDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
    

}


