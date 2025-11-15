namespace SIMS.Core.Classes
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
        public User User { get; set; }
    }
}
