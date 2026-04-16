using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.API;
using SIMS.Core.Classes;
using SIMS.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SimsDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        public UsersController(SimsDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher();
        }



        // VULNERABILITY: Sensitive Data Exposure
        // DESCRIPTION: Der Endpoint gibt alle User vollständig zurück.
        // Dadurch könnten sensible Daten wie Passwort-Hash, E-Mail oder Rolle
        // unnötig an den Client weitergegeben werden.
        // MITIGATION: Nur die wirklich benötigten Felder zurückgeben.
        // Dafür besser ein DTO oder ein vereinfachtes Response-Objekt verwenden.
        // GET: api/Users
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

// VULNERABILITY: Sensitive Data Exposure
// DESCRIPTION: Über die ID kann jeder beliebige Benutzer aufgerufen werden.
// Dadurch könnten auch sensible Daten wie Passwort-Hash, Rolle oder E-Mail sichtbar werden.
// Es wird nicht geprüft, ob der anfragende Benutzer diesen Datensatz überhaupt sehen darf.
// MITIGATION: Den Endpoint absichern und prüfen,
// ob der Benutzer nur seine eigenen Daten oder als Admin fremde Daten sehen darf.

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

// VULNERABILITY: Overposting / Missing Authorization
// DESCRIPTION: Der Endpoint übernimmt das komplette User-Objekt vom Client.
// Dadurch könnten Felder geändert werden, die nicht frei änderbar sein sollten,
// zum Beispiel Rolle, Enabled oder PasswordHash. Außerdem ist keine sichtbare Rechteprüfung vorhanden.
// MITIGATION: Nur erlaubte Felder über ein DTO entgegennehmen und den Endpoint mit Authentifizierung und Rollenprüfung absichern.

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            //Hashen bevor gespeichert wird:
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user.PasswordHash);
            }
            else
            {
                return BadRequest(new { message = "Password is required" });
            }

            //// Optional: prüft ob der Username bereits existiert
            //if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            //{
            //    return Conflict(new { message = "Username already exists" });
            //}

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }


        // VULNERABILITY: Broken Access Control
        // DESCRIPTION: Der Endpoint führt eine sensible Aktion aus,
        // ohne dass eine sichtbare Authentifizierung oder Rollenprüfung vorhanden ist.
        // Dadurch könnte ein Benutzer andere Benutzer löschen, obwohl er dazu nicht berechtigt ist.
        // MITIGATION: Den Endpoint mit Authentifizierung absichern
        // und nur Administratoren das Löschen erlauben.

        // DELETE: api/Users/
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        //um aktiveren und deaktiveren von usern

      // VULNERABILITY: Broken Access Control
     // DESCRIPTION: Der Endpoint führt eine sicherheitsrelevante Aktion aus,
     // ohne dass eine sichtbare Authentifizierung oder Rollenprüfung vorhanden ist.
     // Dadurch könnte ein Benutzer andere Benutzer aktivieren oder deaktivieren,
     // obwohl er dazu nicht berechtigt ist.
     // MITIGATION: Den Endpoint mit Authentifizierung absichern und nur Administratoren das Ändern des Enabled-Status erlauben.
        
        [HttpPatch("{id}/enabled")]
        public async Task<IActionResult> SetUserEnabled(int id, [FromBody] JsonElement body)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (body.TryGetProperty("enabled", out var enabledProp))
            {
                user.Enabled = enabledProp.GetBoolean();
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return BadRequest();
        }

    }
}
