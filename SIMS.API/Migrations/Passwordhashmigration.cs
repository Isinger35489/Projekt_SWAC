using Microsoft.EntityFrameworkCore;
using SIMS.Core.Classes;
using SIMS.Core.Security;

namespace SIMS.API.Migrations
{

    /// Database migration helper to convert existing plain text passwords to BCrypt hashes
    /// Run this ONCE after implementing BCrypt to update existing user passwords

    public class PasswordHashMigration
    {
 
        /// Convert all plain text passwords in database to BCrypt hashes
        /// WARNING: This is a one-time operation and irreversible!

        public static async Task MigratePasswordsAsync(SimsDbContext context)
        {
            try
            {

                var passwordHasher = new PasswordHasher();
                var users = await context.Users.ToListAsync();

                if (!users.Any())
                {

                    return;
                }

                int migratedCount = 0;
                int skippedCount = 0;

                foreach (var user in users)
                {
                    // Check if password is already hashed (BCrypt hashes start with $2a$, $2b$, or $2y$)
                    if (user.PasswordHash.StartsWith("$2"))
                    {
                        skippedCount++;
                        continue;
                    }

                    // Store the plain text password
                    var plainTextPassword = user.PasswordHash;

                    // Hash the plain text password
                    user.PasswordHash = passwordHasher.HashPassword(plainTextPassword);

                    migratedCount++;

                }

                // Save all changes
                if (migratedCount > 0)
                {
                    await context.SaveChangesAsync();

                }
                else
                {
                    Console.WriteLine("No passwords needed migration");
                }
            }
            
            catch
            {
                throw;
            }
        }

        /// Create default users with hashed passwords
        /// Use this for fresh installations or after database reset

        public static async Task SeedUsersAsync(SimsDbContext context)
        {
            try
            {
                if (await context.Users.AnyAsync())
                {
                    
                    return;
                }

              

                var passwordHasher = new PasswordHasher();

                var defaultUsers = new[]
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = passwordHasher.HashPassword("admin"),
                        Email = "admin@sims.local",
                        Role = "admin",
                        Enabled = true
                    },
                    new User
                    {
                        Username = "user",
                        PasswordHash = passwordHasher.HashPassword("user"),
                        Email = "user@sims.local",
                        Role = "user",
                        Enabled = true
                    },
                    new User
                    {
                        Username = "security_officer",
                        PasswordHash = passwordHasher.HashPassword("security"),
                        Email = "security@sims.local",
                        Role = "user",
                        Enabled = true
                    }
                };

                context.Users.AddRange(defaultUsers);
                await context.SaveChangesAsync();

            }
            catch 
            {
                throw;
            }
        }

        /// Verify that all passwords in database are properly hashed

        public static async Task<bool> VerifyPasswordHashesAsync(SimsDbContext context)
        {
            try
            {
                var users = await context.Users.ToListAsync();

                if (!users.Any())
                {
                   
                    return true;
                }

                bool allValid = true;

                foreach (var user in users)
                {
                    // BCrypt hashes should:
                    // 1. Start with $2a$, $2b$, or $2y$
                    // 2. Be 60 characters long
                    bool isValidHash = user.PasswordHash.StartsWith("$2") && user.PasswordHash.Length == 60;

                    if (!isValidHash)
                    {
                       allValid = false;
                    }
                    else
                    {
                       Console.WriteLine("User {Username} has valid BCrypt hash", user.Username);
                    }
                }

                if (allValid)
                {
                    Console.WriteLine("All user passwords are properly hashed");
                }
                else
                {
                    Console.WriteLine("Some users have invalid password hashes - migration may be needed");
                }

                return allValid;
            }
            catch 
            {
                return false;
            }
        }
    }
}

