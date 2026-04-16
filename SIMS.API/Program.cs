using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
//using SIMS.Core;
using SIMS.Core.Classes;
using SIMS.Core.Security;
using StackExchange.Redis;
using SIMS.API.Services;
using SIMS.API.Middleware;

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
/*
VULNERABILITY: Hardcoded Connection String
DESCRIPTION: Die Datenbankverbindung wird direkt aus der Konfiguration gelesen ohne zu prüfen ob sie aus einer sicheren Quelle stammt. 
    In der appsettings.json  steht die Connection String mit SA-Passwort im Klartext.
MITIGATION: Connection String über Umgebungsvariablen oder Docker Secrets injizieren und nicht über appsettings.json
*/
            builder.Services.AddDbContext<SimsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHttpClient<TelegramAlerter>();

            builder.Services.AddEndpointsApiExplorer();
            
            //API Key Header hinzufügen
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "SIMS API",
                    Version = "v1",
                    Description = "Security Incident Management System API"
                });

                // Add API Key header to Swagger
                c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "API Key required for authentication",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Name = "X-API-Key",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }); ;

            // Redis
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

            builder.Services.AddSingleton<IConnectionMultiplexer>(
/*
VULNERABILITY: Redis ohne Authentifizierung
DESCRIPTION: Die Redis-Verbindung wird ohne Passwort oder TLS konfiguriert. Ein Angreifer mit Netzwerkzugriff kann direkt auf den Redis-Store zugreifen und alle Sessions lesen oder manipulieren.
MITIGATION: Redis-Passwort setzen und TLS-Verbindung erzwingen. Verbindungsstring über Umgebungsvariablen oder Secrets Manager injizieren.
*/
                ConnectionMultiplexer.Connect($"{redisHost}:{redisPort},abortConnect=false")
            );
            builder.Services.AddSingleton<RedisSessionService>();

            var app = builder.Build();


            //==================================================================
            // DB-Migration und Passwort-Hash-Migration werden unten ausgeführt,
            // auskommentieren wenn nicht benötigt:
            //==================================================================

            //using (var scope = app.Services.CreateScope())
            //{
            //    var svc = scope.ServiceProvider;
            //    try
            //    {
            //        var context = svc.GetRequiredService<SimsDbContext>();

            //        // Migrations ausführen (legt u.a. Tabelle "Incidents" an)
            //        //Console.WriteLine("Running database migrations...");
            //        context.Database.Migrate();
            //        //Console.WriteLine("Migrations completed successfully!");


            //        // Checkt ob user gehashed sind und macht es dann:
            //        var firstUser = context.Users.FirstOrDefault();
            //        if (firstUser != null && !firstUser.PasswordHash.StartsWith("$2")) //"$2" ist der Anfang von BCrypt-Hashes
            //        {
            //            Console.WriteLine("Found plain text passwords! Converting to BCrypt...");

            //            // Migrieren vorhandener Passwörter:
            //            Migrations.PasswordHashMigration.MigratePasswordsAsync(context).Wait();

            //            Console.WriteLine("Password migration complete!");
            //        }
            //        else
            //        {
            //            Console.WriteLine("Passwords already hashed");
            //        }

            //        //checkt nochmal nach, ob die Passwörter jetzt alle gehashed sind:
            //        var allValid = Migrations.PasswordHashMigration
            //            .VerifyPasswordHashesAsync(context).Result;

            //        if (allValid)
            //        {
            //            Console.WriteLine("All passwords are properly hashed!");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.Error.WriteLine("Fehler beim Initialisieren der DB: " + ex);
            //        throw;
            //    }
            //}
            //
            //==================================================================
            // ENDE DB-Migration und Passwort-Hash-Migration
            //==================================================================




            
            // Configure the HTTP request pipeline.
/*
VULNERABILITY: Offen zugängliche API-Dokumentation über Swagger
DESCRIPTION: Swagger ist zwar auf die Entwicklungsumgebung beschränkt, legt dort aber die
API-Struktur, Endpoints und Parameter offen. Bei Fehlkonfiguration der Umgebung könnte
diese Dokumentation unbeabsichtigt auch außerhalb von Development verfügbar sein.
MITIGATION: Swagger zusätzlich über eine Konfigurationsoption absichern und
außerhalb von Development deaktiviert lassen.
*/
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIMS API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
            app.UseAuthorization();

            // CORS vor den Controllern anwenden ist sauberer
            app.UseCors("AllowWeb");
            app.UseCors("AllowBlazor");

            app.MapControllers();

            app.Run();
        }
    }
}
