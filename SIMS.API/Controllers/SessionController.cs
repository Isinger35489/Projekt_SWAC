using Microsoft.AspNetCore.Mvc;
namespace SIMS.API.Controllers
{
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
