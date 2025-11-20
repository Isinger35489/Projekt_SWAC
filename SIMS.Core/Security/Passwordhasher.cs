using BCrypt.Net;

namespace SIMS.Core.Security
{

    /// Password hashing utility using BCrypt algorithm
    /// Provides secure password hashing and verification for the SIMS project

    public class PasswordHasher
    {
      
        /// Hash a plain text password using BCrypt
 
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Passwort darf nicht NULL oder leer sein", nameof(password));
            }

            // BCrypt.HashPassword automatically generates and includes a salt
            // WorkFactor 12 is a good balance between security and performance
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

 
        /// Checkt das Passwort mit dem hinterlegten Hashwert
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
                //wenn hash format nicht passt, "false" zurücksenden
                return false;
            }
        }


        /// Nochmal überprüfen ob das PW rehashed werden muss, um sicher zu gehen
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