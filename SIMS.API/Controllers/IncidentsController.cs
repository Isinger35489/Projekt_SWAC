using Microsoft.AspNetCore.Mvc;
using SIMS.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIMS.API.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class IncidentsController : ControllerBase
        {
            private static List<Incident> _incidents = new List<Incident>();

            [HttpGet]
            public IEnumerable<Incident> Get() => _incidents;

            [HttpPost]
            public IActionResult Post([FromBody] Incident incident)
            {
                _incidents.Add(incident);
                return Ok();
            }
        }
}


