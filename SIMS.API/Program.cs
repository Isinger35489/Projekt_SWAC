using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
//using SIMS.Core;
using SIMS.Core.Classes;
using SIMS.Core.Security;
using StackExchange.Redis;
using SIMS.API.Services;


namespace SIMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            //anbindung des SIMS:WEB Ports (KI Zeugs):
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWeb", policy =>
                {
                    policy.WithOrigins("https://localhost:7167")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazor", policy =>
                {
                    policy.WithOrigins("https://localhost:7167")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // appsettings-api.json laden (vor AddDbContext)
            //Datenbank service hinzuf�gen
            builder.Services.AddDbContext<SimsDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHttpClient<TelegramAlerter>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            


            //container starten: docker run -d --name redis-1 -p 6379:6379 redis:latest
            //Anbindung zur Redis DB (danke KI):
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

            //variabler port f�r redis statt fixer port
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect($"{redisHost}:{redisPort},abortConnect=false")
            );
                        //RegisSessionService einbinden
            builder.Services.AddSingleton<RedisSessionService>();

            //nach hier d�rfen keine Builds mehr vorkommen
            var app = builder.Build();

            // testen vom PasswordHasher. Kommt gleich oben in der API Konsole
            //var hasher = new PasswordHasher();
            //var hashed = hasher.HashPassword("testpassword");
            //bool isValid = hasher.VerifyPassword("testpassword", hashed);
            //Console.WriteLine($"Hash: {hashed}");
            //Console.WriteLine($"Valid: {isValid}"); // sollte "True" ausgeben

            //hinzuf�gen von Testdaten in die SQL Datenbank:
            using (var scope = app.Services.CreateScope())
            {
                var svc = scope.ServiceProvider;
                try//danke KI
                {
                    var context = svc.GetRequiredService<SimsDbContext>();

                    ////macht die Migrations, falls es �nderungen gibt.
                    //Console.WriteLine("Running database migrations...");
                    //context.Database.Migrate();
                    //Console.WriteLine("Migrations completed successfully!");


                    //User und Incident Daten h�ndisch hinzuf�gen, kann auskommentiert werden, wenn nicht mehr ben�tigt:
                    //context.Incidents.Add(new Incident
                    //{
                    //    ReporterId = 8,
                    //    HandlerId = 12,
                    //    Description = "Server down",
                    //    Severity = "High",
                    //    Status = "In Work",
                    //    CVE = "CVE129",
                    //    EscalationLevel = 2,
                    //    System = "WebServer01",
                    //    CreatedAt = DateTime.Now
                    //});

                    //context.Users.Add(new User
                    //{
                    //    Username = "adminia",
                    //    PasswordHash = "geheim",
                    //    Email = "sdlkfjas@klasdjf.com",
                    //    Role = "admin",
                    //    Enabled = true,
                    //    CreatedAt = DateTime.Now
                    //});

                    //context.Users.Add(new User
                    //{
                    //    Username = "peter",
                    //    PasswordHash = "lustig",
                    //    Email = "sdlkfjas@klasdjf.com",
                    //    Role = "user",
                    //    Enabled = true,
                    //    CreatedAt = DateTime.Now
                    //});

                    context.SaveChanges();

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Fehler beim Initialisieren/Seed der DB: " + ex);
                    throw;
                }

            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();


            //KI Zeugs:
            app.UseCors("AllowWeb");

            app.UseCors("AllowBlazor");

            app.Run();
        }
    }
}
