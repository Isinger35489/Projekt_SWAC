//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Core.Classes;
using SIMS.Core.Security;
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
        private readonly PasswordHasher _passwordHasher;
        public LoginController(SimsDbContext context, RedisSessionService redisSession)
        {
            _context = context;
            _redisSession = redisSession;
            _passwordHasher = new PasswordHasher();
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

            // Password checken
            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {                
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
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

        [HttpGet("validate")]
        public IActionResult Validate([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest(new { success = false, message = "SessionId erforderlich" });

            var sessionData = _redisSession.GetSession($"session:{sessionId}");
            if (string.IsNullOrEmpty(sessionData))
                return NotFound(new { success = false, message = "Ungültige oder abgelaufene Session" });

            return Ok(new { success = true, message = "Session gültig", sessionData });
        }

        //[HttpPost("logout")]
        //public IActionResult Logout([FromBody] LogoutRequest request)
        //{
        //    // In production, you would delete the session from Redis here
        //    return Ok(new { success = true, message = "Logout successful" });
        //}
    }
}

