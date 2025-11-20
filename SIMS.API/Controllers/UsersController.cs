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

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

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
