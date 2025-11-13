using Microsoft.AspNetCore.Mvc;
using SIMS.Core;

namespace SIMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static List<User> _users = new List<User>();

        [HttpGet]
        public IEnumerable<User> Get() => _users;

        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            _users.Add(user);
            return Ok();
        }
    }

}
