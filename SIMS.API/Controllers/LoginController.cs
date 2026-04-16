
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Core.Classes;
using SIMS.Core.Security;


namespace SIMS.API.Controllers
{

/*
VULNERABILITY: Broken Access Control
DESCRIPTION: Keine Authorisierung auf Controller-Ebene, der Validate-Endpoint ist vollständig offen und kann von jedem ohne Authentifizierung aufgerufen werden.
MITIGATION: [Authorize] auf den Validate-Endpoint setzen damit nur authentifizierte User ihre Session validieren können.
*/
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

/*
VULNERABILITY: Missing Brute-Force Protection
DESCRIPTION: Es gibt keine Begrenzung der Login-Versuche obwohl MaxLoginAttempts: 5 
in der appsettings.json konfiguriert ist. Ein Angreifer kann unbegrenzt Passwörter durchprobieren ohne gesperrt zu werden.
MITIGATION: Fehlversuche in Redis zählen und den Account nach Erreichen von MaxLoginAttempts temporär sperren. Konfigurationswert aus appsettings.json tatsächlich verwenden.
*/
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


/*
VULNERABILITY: Insecure Session Data Format
DESCRIPTION: Session-Daten werden als manipulierbarer Klartext-String gespeichert. Wer Redis-Zugriff hat kann die Rolle direkt lesen und über den offenen 
SessionController manipulieren.
MITIGATION: Session-Daten als strukturiertes JSON serialisieren oder auf JWT Bearer Tokens umsteigen die kryptografisch signiert sind.
*/
            // Create session in Redis
            var sessionId = Guid.NewGuid().ToString();
            var sessionData = $"{user.Id}|{user.Username}|{user.Role}";
/*
VULNERABILITY: Missing Session Expiry
DESCRIPTION: Sessions werden ohne Ablaufdatum gesetzt obwohl SessionExpirationMinutes: 60 in der appsettings.json konfiguriert ist. Eine gestohlene Session ist damit dauerhaft gültig.
MITIGATION: TTL beim Setzen der Session übergeben und den konfigurierten SessionExpirationMinutes-Wert tatsächlich verwenden.
*/
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

// VULNERABILITY: Insecure Session Handling
// DESCRIPTION: Die Session-ID wird in der URL übergeben.
// Dadurch kann sie leichter in Browser-Verlauf, Logs oder Proxies auftauchen.
// Wenn jemand die Session-ID sieht, könnte er die Session missbrauchen.
// MITIGATION: Session-IDs nicht in der URL übertragen.
// Stattdessen Cookies oder sichere Header verwenden.

        [HttpGet("validate")]
        public IActionResult Validate([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest(new { success = false, message = "SessionId erforderlich" });

            var sessionData = _redisSession.GetSession($"session:{sessionId}");
            if (string.IsNullOrEmpty(sessionData))
                return NotFound(new { success = false, message = "Ungültige oder abgelaufene Session" });

// VULNERABILITY: Sensitive Data Exposure
// DESCRIPTION: Der Endpoint gibt interne Session-Daten direkt an den Client zurück.
// Dadurch werden unnötige Informationen über Benutzer oder Rollen offengelegt.
// MITIGATION: Nur zurückgeben, ob die Session gültig ist,
// aber keine internen Session-Daten mitsenden.

            return Ok(new { success = true, message = "Session gültig", sessionData });
        }

    }
}

