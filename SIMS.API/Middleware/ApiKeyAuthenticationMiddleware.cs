using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SIMS.API.Middleware
{

    /// Middleware to validate API Key in request headers
    /// Ensures only authorized clients (like SIMS.Web) can access the API

    public class ApiKeyAuthenticationMiddleware
    {
        private const string API_KEY_HEADER_NAME = "X-API-Key";
        private readonly RequestDelegate _next;
     
        private readonly string _validApiKey;

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration
            )
 
        {
            _next = next;
            _validApiKey = configuration["Security:ApiKey"]
                ?? throw new InvalidOperationException("API Key not configured in appsettings.json");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for Swagger/development endpoints
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/swagger") || path.Contains("/health"))
            {
                await _next(context);
                return;
            }

            // Check if API Key is present in header
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var receivedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            // Validate API Key
            if (!IsValidApiKey(receivedApiKey))
            {
                
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            // API Key is valid, continue to next middleware
            
            await _next(context);
        }

        private bool IsValidApiKey(string? receivedApiKey)
        {
            if (string.IsNullOrWhiteSpace(receivedApiKey))
            {
                return false;
            }

            // Constant-time comparison to prevent timing attacks
            return CryptographicEquals(receivedApiKey, _validApiKey);
        }


        /// Constant-time string comparison to prevent timing attacks

        private bool CryptographicEquals(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var areEqual = true;
            for (int i = 0; i < a.Length; i++)
            {
                areEqual &= (a[i] == b[i]);
            }

            return areEqual;
        }
    }
}