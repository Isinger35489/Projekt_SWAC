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
        

            var apiBase = builder.Configuration["ApiSettings:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("API_URI_BASE")
                ?? "https://localhost:7168";

            //API Key aus appsetting.json holen
            var apiKey = builder.Configuration["ApiSettings:ApiKey"]
                ?? throw new InvalidOperationException("API Key not configured in appsettings.json");

            //Verbindung an SQL DB, DB Service starten
            builder.Services.AddDbContext<SimsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnetion")));

            //Razor Service hinzufügen:
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

                       
            builder.Services.AddHttpClient("SecureApiClient", client =>
            {
                client.BaseAddress = new Uri(apiBase);
                client.Timeout = TimeSpan.FromSeconds(30);

                // API key zu allen Requests hinzufügen:
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                // zusätzliche Security Header: danke KI
                client.DefaultRequestHeaders.Add("User-Agent", "SIMS.Web/1.0");
            })
           .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
           {
              
               //selbst signiertes Zertifikat übernehmen. Nicht ideal aber besser gehts nicht
               ServerCertificateCustomValidationCallback =
                   HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
           });

            // Register named HttpClient for use in components
            builder.Services.AddScoped(sp =>
            {
                var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return clientFactory.CreateClient("SecureApiClient");
            });

            builder.Services.AddScoped<LoginState>();


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
