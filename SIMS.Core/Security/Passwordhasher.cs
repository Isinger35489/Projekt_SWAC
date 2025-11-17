using BCrypt.Net;

namespace SIMS.Core.Security
{
    /// <summary>
    /// Password hashing utility using BCrypt algorithm
    /// Provides secure password hashing and verification for the SIMS project
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// Hash a plain text password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password to hash</param>
        /// <returns>BCrypt hashed password with salt</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // BCrypt.HashPassword automatically generates and includes a salt
            // WorkFactor 12 is a good balance between security and performance
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verify a plain text password against a BCrypt hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="hashedPassword">BCrypt hashed password from database</param>
        /// <returns>True if password matches, false otherwise</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            try
            {
                // BCrypt.Verify handles the salt extraction automatically
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                // If hash format is invalid, return false instead of throwing
                return false;
            }
        }

        /// <summary>
        /// Check if a password needs to be rehashed (e.g., if work factor has changed)
        /// </summary>
        /// <param name="hashedPassword">Current hashed password</param>
        /// <param name="targetWorkFactor">Desired work factor (default: 12)</param>
        /// <returns>True if password should be rehashed</returns>
        public bool NeedsRehash(string hashedPassword, int targetWorkFactor = 12)
        {
            try
            {
                return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, targetWorkFactor);
            }
            catch (Exception)
            {
                // If we can't determine, assume it needs rehashing
                return true;
            }
        }
    }
}