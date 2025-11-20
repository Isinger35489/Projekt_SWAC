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

            // DB-Migration + optional Seed
            using (var scope = app.Services.CreateScope())
            {
                var svc = scope.ServiceProvider;
                try
                {
                    var context = svc.GetRequiredService<SimsDbContext>();

                    // 💾 Migrations ausführen (legt u.a. Tabelle "Incidents" an)
                    Console.WriteLine("Running database migrations...");
                    context.Database.Migrate();
                    Console.WriteLine("Migrations completed successfully!");

                    // Wenn du nichts seedest, brauchst du SaveChanges hier eigentlich nicht mehr.
                    // context.SaveChanges();
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

            // CORS vor den Controllern anwenden ist sauberer
            app.UseCors("AllowWeb");
            app.UseCors("AllowBlazor");

            app.MapControllers();

            app.Run();
        }
    }
}
