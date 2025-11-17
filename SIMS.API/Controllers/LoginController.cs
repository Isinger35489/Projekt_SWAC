//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Core.Classes;
//using SIMS.Core;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class LoginController : ControllerBase
    {
        private readonly SimsDbContext _context;
        private readonly RedisSessionService _redisSession;

        public LoginController(SimsDbContext context, RedisSessionService redisSession)
        {
            _context = context;
            _redisSession = redisSession;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) //ist angegeben 
        {
            // Validate input
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Username and password are required"
                });
            }

            // Find user in database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid username or password"
                });
            }

            // Check if user is enabled
            if (!user.Enabled)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Your account has been disabled. Please contact administrator."
                });
            }

            // Verify password
            // NOTE: For university project - in production use BCrypt!
            // This compares plain text - NOT SECURE for real apps!
            if (user.PasswordHash != request.Password)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid username or password"
                });
            }

            // Create session in Redis
            var sessionId = Guid.NewGuid().ToString();
            var sessionData = $"{user.Id}|{user.Username}|{user.Role}";
            _redisSession.SetSession($"session:{sessionId}", sessionData);

            // Return success response
            return Ok(new
            {
                Success = true,
                Message = "Login successful",
                sessionId = sessionId,
                User = new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        //[HttpPost("logout")]
        //public IActionResult Logout([FromBody] LogoutRequest request)
        //{
        //    // In production, you would delete the session from Redis here
        //    return Ok(new { success = true, message = "Logout successful" });
        //}
    }
}

//    public class LoginController : ControllerBase
//    {
//        private readonly SimsDbContext _context;
//        private readonly RedisSessionService _redisSession;
//        public LoginController(SimsDbContext context, RedisSessionService redisSession)
//        {
//            _context = context;
//            _redisSession = redisSession;

//        }

//        public class LoginRequest
//        {
//            public string Username { get; set; }
//            public string Password { get; set; }
//        }

//        public class LoginResponse
//        {
//            public bool Success { get; set; }
//            public string Message { get; set; }
//            public string SessionId { get; set; }
//            public UserInfo User { get; set; }
//        }

//        public class UserInfo
//        {
//            public int Id { get; set; }
//            public string Username { get; set; }
//            public string Email { get; set; }
//            public string Role { get; set; }
//        }

//        public class LogoutRequest
//        {
//            public string SessionId { get; set; }
//        }

//        public class PasswordVerifyRequest
//        {
//            public string Username { get; set; }
//            public string Password { get; set; }
//        }

//        // ========================================================================
//        // HAUPT-ENDPOINTS
//        // ========================================================================

//        // POST: api/Login
//        [HttpPost]
//        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
//        {
//            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
//            {
//                return BadRequest(new LoginResponse
//                {
//                    Success = false,
//                    Message = "Benutzername und Passwort sind erforderlich"
//                });
//            }

//            // Suche User in Datenbank
//            var user = await _context.Users
//                .FirstOrDefaultAsync(u => u.Username == request.Username);

//            if (user == null)
//            {
//                return Unauthorized(new LoginResponse
//                {
//                    Success = false,
//                    Message = "Ungültiger Benutzername oder Passwort"
//                });
//            }

//            // Prüfe ob User aktiviert ist
//            if (!user.Enabled)
//            {
//                return Unauthorized(new LoginResponse
//                {
//                    Success = false,
//                    Message = "Ihr Konto wurde deaktiviert. Bitte kontaktieren Sie den Administrator."
//                });
//            }

//            // Passwort prüfen
//            // HINWEIS: In Produktion solltest du BCrypt oder ähnliches verwenden!
//            // Für das Uni-Projekt vergleichen wir direkt (NICHT sicher!)
//            if (user.PasswordHash != request.Password)
//            {
//                return Unauthorized(new LoginResponse
//                {
//                    Success = false,
//                    Message = "Ungültiger Benutzername oder Passwort"
//                });
//            }

//            // Erstelle Session in Redis
//            var sessionId = Guid.NewGuid().ToString();
//            var sessionData = $"{user.Id}|{user.Username}|{user.Role}";

//            _redisSession.SetSession($"session:{sessionId}", sessionData);

//            // Login erfolgreich
//            return Ok(new LoginResponse
//            {
//                Success = true,
//                Message = "Login erfolgreich",
//                SessionId = sessionId,
//                User = new UserInfo
//                {
//                    Id = user.Id,
//                    Username = user.Username,
//                    Email = user.Email,
//                    Role = user.Role
//                }
//            });
//        }

//        // POST: api/Login/logout
//        [HttpPost("logout")]
//        public IActionResult Logout([FromBody] LogoutRequest request)
//        {
//            if (!string.IsNullOrEmpty(request.SessionId))
//            {
//                // Session aus Redis löschen würde hier passieren
//                // Benötigt DeleteSession-Methode in RedisSessionService
//            }

//            return Ok(new { Success = true, Message = "Logout erfolgreich" });
//        }

//        // GET: api/Login/validate
//        [HttpGet("validate")]
//        public IActionResult ValidateSession([FromQuery] string sessionId)
//        {
//            if (string.IsNullOrEmpty(sessionId))
//            {
//                return BadRequest(new { Success = false, Message = "SessionId erforderlich" });
//            }

//            var sessionData = _redisSession.GetSession($"session:{sessionId}");

//            if (string.IsNullOrEmpty(sessionData))
//            {
//                return Unauthorized(new { Success = false, Message = "Ungültige oder abgelaufene Session" });
//            }

//            return Ok(new { Success = true, Message = "Session gültig", SessionData = sessionData });
//        }





//        //// GET: api/<LoginController>
//        //[HttpGet]
//        //public IEnumerable<string> Get()
//        //{
//        //    return new string[] { "value1", "value2" };
//        //}

//        //// GET api/<LoginController>/5
//        //[HttpGet("{id}")]
//        //public string Get(int id)
//        //{
//        //    return "value";
//        //}

//        //// POST api/<LoginController>
//        //[HttpPost]
//        //public void Post([FromBody] string value)
//        //{

//        //}

//        //// PUT api/<LoginController>/5
//        //[HttpPut("{id}")]
//        //public void Put(int id, [FromBody] string value)
//        //{
//        //}

//        //// DELETE api/<LoginController>/5
//        //[HttpDelete("{id}")]
//        //public void Delete(int id)
//        //{

//        //}
//    }
//}
