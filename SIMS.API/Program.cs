using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
namespace SIMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //weil ohne dass die umgeänderte variante von appsetings nicht übernommen werden kann, da im web auch eine appsettings vorhanden ist,
            //kommt es sonst beim docker build zu einem fehler (normale bei multi-container)

            builder.Configuration.AddJsonFile("appsettings-api.json", optional: true, reloadOnChange: true);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Datenbank service hinzufügen
            builder.Services.AddDbContext<SimsDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Lösung für Regis Problem local wird localhost verwendet, für redis wird port 6379 verwendet
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

            //regis integrieren auf fixen port
            // builder.Services.AddSingleton<IConnectionMultiplexer>
            //   (                  ConnectionMultiplexer.Connect("redis:6379"));

            //variabler port für redis statt fixer port
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}")
            );

            //RegisSessionService einbinden
            builder.Services.AddSingleton<RedisSessionService>();

            //nach hier dürfen keine Builds mehr vorkommen
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
