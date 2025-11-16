using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SIMS.Core.Classes;

public class LoginState
{
        private ProtectedSessionStorage _sessionStorage;

    public int Id { get; set; }
    public string Username { get; set; }
    public RoleType Role { get; set; }

    public string SessionId { get; set; } = string.Empty;
    public bool IsLoggedIn => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(SessionId);

    public LoginState(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    // State im Browser speichern
    public async Task SaveToStorage()
    {
        try
        {
            await _sessionStorage.SetAsync("userId", Id);
            await _sessionStorage.SetAsync("username", Username);
            await _sessionStorage.SetAsync("userRole", Role.ToString());
            await _sessionStorage.SetAsync("sessionId", SessionId);
            Console.WriteLine($"Saved to storage - User: {Username}, SessionId: {SessionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
        }
    }

    // State aus Browser laden
    public async Task<bool> LoadFromStorage()
    {
        try
        {
            var idResult = await _sessionStorage.GetAsync<int>("userId");
            var usernameResult = await _sessionStorage.GetAsync<string>("username");
            var roleResult = await _sessionStorage.GetAsync<string>("userRole");
            var sessionIdResult = await _sessionStorage.GetAsync<string>("sessionId");
            Console.WriteLine($"Load attempt - Id: {idResult.Success}, Username: {usernameResult.Success}, Role: {roleResult.Success}, SessionId: {sessionIdResult.Success}");
            
            
            if (idResult.Success && usernameResult.Success &&
                roleResult.Success && sessionIdResult.Success)
            {
                Id = idResult.Value;
                Username = usernameResult.Value;
                Role = Enum.Parse<RoleType>(roleResult.Value);
                SessionId = sessionIdResult.Value;
                Console.WriteLine($"Loaded from storage - User: {Username}, SessionId: {SessionId}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden des States: {ex.Message}");
        }

        return false;
    }

    // State löschen (Logout)
    public async Task ClearStorage()
    {
        try { 
        await _sessionStorage.DeleteAsync("userId");
        await _sessionStorage.DeleteAsync("username");
        await _sessionStorage.DeleteAsync("userRole");
        await _sessionStorage.DeleteAsync("sessionId");

        Id = 0;
        Username = string.Empty;
        Role = RoleType.User;
        SessionId = string.Empty;
        Console.WriteLine("Storage cleared");
    }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Löschen: {ex.Message}");
            }
    }
}


