using Microsoft.EntityFrameworkCore;
using SIMS.Core.Classes;
using SIMS.Core.Security;

namespace SIMS.API.Migrations
{
    /// <summary>
    /// Database migration helper to convert existing plain text passwords to BCrypt hashes
    /// Run this ONCE after implementing BCrypt to update existing user passwords
    /// </summary>
    public class PasswordHashMigration
    {
        /// <summary>
        /// Convert all plain text passwords in database to BCrypt hashes
        /// WARNING: This is a one-time operation and irreversible!
        /// </summary>
        public static async Task MigratePasswordsAsync(SimsDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting password hash migration...");

                var passwordHasher = new PasswordHasher();
                var users = await context.Users.ToListAsync();

                if (!users.Any())
                {
                    logger.LogInformation("No users found to migrate");
                    return;
                }

                int migratedCount = 0;
                int skippedCount = 0;

                foreach (var user in users)
                {
                    // Check if password is already hashed (BCrypt hashes start with $2a$, $2b$, or $2y$)
                    if (user.PasswordHash.StartsWith("$2"))
                    {
                        logger.LogInformation("User {Username} already has hashed password, skipping", user.Username);
                        skippedCount++;
                        continue;
                    }

                    // Store the plain text password
                    var plainTextPassword = user.PasswordHash;

                    // Hash the plain text password
                    user.PasswordHash = passwordHasher.HashPassword(plainTextPassword);

                    migratedCount++;
                    logger.LogInformation("Migrated password for user: {Username}", user.Username);
                }

                // Save all changes
                if (migratedCount > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Password migration completed: {Migrated} migrated, {Skipped} skipped",
                        migratedCount, skippedCount);
                }
                else
                {
                    logger.LogInformation("No passwords needed migration");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during password migration");
                throw;
            }
        }

        /// <summary>
        /// Create default users with hashed passwords
        /// Use this for fresh installations or after database reset
        /// </summary>
        public static async Task SeedUsersAsync(SimsDbContext context, ILogger logger)
        {
            try
            {
                if (await context.Users.AnyAsync())
                {
                    logger.LogInformation("Users already exist, skipping seeding");
                    return;
                }

                logger.LogInformation("Seeding default users with hashed passwords...");

                var passwordHasher = new PasswordHasher();

                var defaultUsers = new[]
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = passwordHasher.HashPassword("admin123"),
                        Email = "admin@sims.local",
                        Role = "admin",
                        Enabled = true
                    },
                    new User
                    {
                        Username = "user",
                        PasswordHash = passwordHasher.HashPassword("user123"),
                        Email = "user@sims.local",
                        Role = "user",
                        Enabled = true
                    },
                    new User
                    {
                        Username = "security_officer",
                        PasswordHash = passwordHasher.HashPassword("security123"),
                        Email = "security@sims.local",
                        Role = "user",
                        Enabled = true
                    }
                };

                context.Users.AddRange(defaultUsers);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully seeded {Count} users with hashed passwords", defaultUsers.Length);

                // Log credentials for testing (ONLY in development!)
                logger.LogWarning("=== DEFAULT USER CREDENTIALS (Development Only) ===");
                logger.LogWarning("Admin: username=admin, password=admin123");
                logger.LogWarning("User: username=user, password=user123");
                logger.LogWarning("Security Officer: username=security_officer, password=security123");
                logger.LogWarning("================================================");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during user seeding");
                throw;
            }
        }

        /// <summary>
        /// Verify that all passwords in database are properly hashed
        /// </summary>
        public static async Task<bool> VerifyPasswordHashesAsync(SimsDbContext context, ILogger logger)
        {
            try
            {
                var users = await context.Users.ToListAsync();

                if (!users.Any())
                {
                    logger.LogWarning("No users found in database");
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
                        logger.LogError("User {Username} has invalid password hash format!", user.Username);
                        allValid = false;
                    }
                    else
                    {
                        logger.LogInformation("User {Username} has valid BCrypt hash", user.Username);
                    }
                }

                if (allValid)
                {
                    logger.LogInformation("All user passwords are properly hashed");
                }
                else
                {
                    logger.LogError("Some users have invalid password hashes - migration may be needed");
                }

                return allValid;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during password hash verification");
                return false;
            }
        }
    }
}

// ===================================================================
// USAGE IN Program.cs
// ===================================================================

/*

Add this to your Program.cs in SIMS.API after database migration:

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<SimsDbContext>();

    try
    {
        // Apply EF migrations first
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();

        // Option 1: Seed new users (fresh database)
        await PasswordHashMigration.SeedUsersAsync(context, logger);

        // Option 2: Migrate existing plain text passwords (existing database)
        // await PasswordHashMigration.MigratePasswordsAsync(context, logger);

        // Verify all passwords are properly hashed
        await PasswordHashMigration.VerifyPasswordHashesAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}

*/