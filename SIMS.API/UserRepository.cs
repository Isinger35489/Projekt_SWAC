using SIMS.Core;
using SIMS.Core.Classes;

namespace SIMS.API
{
    public class UserRepository : IRepository<User>
    {
        private List<User> _users = new List<User>();

        public User GetById(int id) => _users.FirstOrDefault(u => u.Id == id);
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
