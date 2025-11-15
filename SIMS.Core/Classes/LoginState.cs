using SIMS.Core.Classes;

public class LoginState
{
    public int Id { get; set; }
    public string Username { get; set; }
    public RoleType Role { get; set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(Username);
}

