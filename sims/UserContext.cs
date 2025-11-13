using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;


public class UserContext : DbContext
{
    public DbSet<User> User{ get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //Für die Verwendung von "LocalDB":
        optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=SchoolDB;Trusted_Connection=True;");
        
        //docker run -p 6379:6379 --name redisDB1 --hostname redisDB1 -v redis_data:/data -d redis:alpine redis-server --requirepass Adm1234! --save 60 1 --loglevel warning
        //docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Adm1234!" -p 1433:1433 --name sq --hostname sql1 -d mcr.microsoft.com/mssql/server:2017-latest


        //Für die Verwendung von Docker
        //ACHTUNG! Container muss natürlich gestartet sein
        //optionsBuilder.UseSqlServer(@"Data Source=localhost;User ID=sa;Password=Adm1234!;Database=SchoolDB;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
    }

}