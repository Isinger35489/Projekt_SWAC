using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SIMS.Core.Classes;

namespace SIMS.API.Services
{
    public class TelegramAlerter
    {
        private readonly HttpClient _httpClient;
        private readonly string _botToken;
        private readonly string _chatId;

        public TelegramAlerter(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _botToken = config["Telegram:BotToken"];
            if (string.IsNullOrWhiteSpace(_botToken))
                throw new InvalidOperationException("Telegram:BotToken nicht konfiguriert oder leer.");

            _chatId = config["Telegram:ChatId"];
            if (string.IsNullOrWhiteSpace(_chatId))
                throw new InvalidOperationException("Telegram:ChatId nicht konfiguriert oder leer.");
        }

        public async Task SendIncidentCreatedAsync(Incident incident)
        {
            if (incident == null)
                throw new ArgumentNullException(nameof(incident));

            var description = string.IsNullOrWhiteSpace(incident.Description)
                ? "(keine Beschreibung angegeben)"
                : incident.Description;

            var text =
                $"[SIMS] Neues Incident #{incident.Id}\n" +
                $"System: {incident.System}\n" +
                $"Severity: {incident.Severity}\n" +
                $"Status: {incident.Status}\n" +
                $"Beschreibung: {description}";

            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var data = new Dictionary<string, string>
            {
                ["chat_id"] = _chatId,
                ["text"] = text
            };

            using var content = new FormUrlEncodedContent(data);

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine(
                        $"Telegram-Alert fehlgeschlagen ({(int)response.StatusCode} {response.StatusCode}): {body}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Telegram-Alert Exception: {ex}");
            }
        }
    }
}
