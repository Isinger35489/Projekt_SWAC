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

            //Datenbank service hinzufügen
            builder.Services.AddDbContext<SimsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHttpClient<TelegramAlerter>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Redis
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect($"{redisHost}:{redisPort},abortConnect=false")
            );
            builder.Services.AddSingleton<RedisSessionService>();

            var app = builder.Build();


            //==================================================================
            // DB-Migration und Passwort-Hash-Migration werden unten ausgeführt,
            // auskommentieren wenn nicht benötigt:
            //==================================================================

            using (var scope = app.Services.CreateScope())
            {
                var svc = scope.ServiceProvider;
                try
                {
                    var context = svc.GetRequiredService<SimsDbContext>();

                    // Migrations ausführen (legt u.a. Tabelle "Incidents" an)
                    //Console.WriteLine("Running database migrations...");
                    context.Database.Migrate();
                    //Console.WriteLine("Migrations completed successfully!");


                    // Checkt ob user gehashed sind und macht es dann:
                    var firstUser = context.Users.FirstOrDefault();
                    if (firstUser != null && !firstUser.PasswordHash.StartsWith("$2")) //"$2" ist der Anfang von BCrypt-Hashes
                    {
                        Console.WriteLine("Found plain text passwords! Converting to BCrypt...");

                        // Migrieren vorhandener Passwörter:
                        Migrations.PasswordHashMigration.MigratePasswordsAsync(context).Wait();

                        Console.WriteLine("Password migration complete!");
                    }
                    else
                    {
                        Console.WriteLine("Passwords already hashed");
                    }

                    //checkt nochmal nach, ob die Passwörter jetzt alle gehashed sind:
                    var allValid = Migrations.PasswordHashMigration
                        .VerifyPasswordHashesAsync(context).Result;

                    if (allValid)
                    {
                        Console.WriteLine("All passwords are properly hashed!");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Fehler beim Initialisieren der DB: " + ex);
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

            // CORS vor den Controllern anwenden ist sauberer
            app.UseCors("AllowWeb");
            app.UseCors("AllowBlazor");

            app.MapControllers();

            app.Run();
        }
    }
}
