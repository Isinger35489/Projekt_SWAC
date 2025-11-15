using SIMS.Core;

public class LoginState
{
    public string Username { get; set; }
    public RoleType Role { get; set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(Username);
}

