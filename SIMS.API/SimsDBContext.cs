using Microsoft.EntityFrameworkCore;
//using SIMS.API;
//using SIMS;
using SIMS.Core.Classes;

namespace SIMS.API
{
    public class SimsDbContext : DbContext
    {
        public SimsDbContext(DbContextOptions<SimsDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
    }
}
