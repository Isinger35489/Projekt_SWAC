using Microsoft.EntityFrameworkCore;
using SIMS.Core;
using System.Collections.Generic;
namespace SIMS.API
{
    public class SimsDbContext : DbContext
    {
        public SimsDbContext(DbContextOptions<SimsDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Für die Verwendung von "LocalDB":
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;User ID=sa; Password=YourStrong@Passw@rd; Database=db-1;Trusted_Connection=True;");

        }
    }
}
