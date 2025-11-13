using Microsoft.EntityFrameworkCore;
using SIMS.Core;
using SIMS.API;
using SIMS;

namespace SIMS.API
{
    public class SimsDbContext : DbContext
    {
        public SimsDbContext(DbContextOptions<SimsDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
    }
}
