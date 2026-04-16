using SIMS.Core;
using SIMS.Core.Classes;

namespace SIMS.API
{
    public class UserRepository : IRepository<User>
    {
        private List<User> _users = new List<User>();
/*
VULNERABILITY: Keine Authentifizierung oder Autorisierung
DESCRIPTION: Alle Methoden sind vollständig offen. Es wird kein Aufrufer geprüft ob er berechtigt ist User abzufragen, hinzuzufügen oder zu löschen.
Hinweis: Diese Klasse ist aktuell nicht aktiv eingebunden. Das Muster zeigt jedoch ein grundlegendes Design-Problem, das im UsersController identisch 
auftritt und dort produktiv ist.
MITIGATION: Zugriffskontrolle auf Controller-Ebene sicherstellen und sichergehen dass das Repository nie direkt ohne Authentifizierung erreichbar ist.
*/
        public User GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

/*
VULNERABILITY: Sensitive Data Exposure
DESCRIPTION: GetAll() und GetById() geben das vollständige User-Objekt zurück inklusive PasswordHash, Rolle und anderen sensiblen Feldern. 
Dieses Problem tritt identisch im produktiven UsersController auf. GET /api/users gibt alle User inklusive PasswordHash direkt aus der 
Datenbank zurück.
MITIGATION: API-Responses auf die notwendigen Felder beschränken. PasswordHash, interne IDs und sensible Felder dürfen nie an den 
Client zurückgegeben werden. Eine eigene Ausgabe-Klasse mit nur erlaubten Feldern verwenden.
*/
        public IEnumerable<User> GetAll() => _users;
        public void Add(User user) => _users.Add(user);
        public void Update(User user)
        {
            var old = GetById(user.Id);
            if (old != null)
            {
                old.Username = user.Username;
               
            }
        }
        public void Delete(int id) => _users.RemoveAll(u => u.Id == id);
    }

}
