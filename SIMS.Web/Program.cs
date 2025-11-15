using Microsoft.EntityFrameworkCore;
using SIMS.API;
using SIMS.Core;
using SIMS.Web.Components;

namespace SIMS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            // OPTIONAL: DbContext nur falls Web direkt auf DB zugreift
            // Wenn nur über API, dann diese Zeile entfernen
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<LoginState>();
            builder.Services.AddDbContext<SimsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnetion")));

            var apiBase = Environment.GetEnvironmentVariable("API_URI_BASE") ?? "https://localhost:7168";
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(apiBase),
                Timeout = TimeSpan.FromSeconds(30)
            });

            builder.Services.AddScoped<LoginState>();


            //
            //builder.Services.AddScoped(sp => new HttpClient
            //{
            //    BaseAddress = new Uri("https://localhost:5001") 
            //});

            // für port 5000 beides gleichzeitig geht nicht
            // builder.Services.AddScoped(sp => new HttpClient
            //{
            //   BaseAddress = new Uri("https://localhost:5000")
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
     
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAntiforgery();

            
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
