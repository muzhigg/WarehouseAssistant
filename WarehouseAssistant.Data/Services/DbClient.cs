using Microsoft.Extensions.Logging;
using Supabase.Postgrest;
using WarehouseAssistant.Data.Interfaces;

namespace WarehouseAssistant.Data.Services;

public class DbClient : IDbClient
{
    private readonly ILogger<DbClient> _logger;
    private readonly Client            _client;
    
    public DbClient(string url, string apiKey, ILogger<DbClient> logger)
    {
        _logger = logger;
        _client = new Client(url, new ClientOptions
        {
            Schema = "public",
            Headers = new Dictionary<string, string>()
            {
                { "apiKey", apiKey }
            },
        });
        _client.AddDebugHandler((sender, message, exception) =>
        {
            if (exception != null)
                _logger.LogError(exception, message);
            else
                _logger.LogDebug(message);
        });
    }
    
    public void SetAuthBearer(string token)
    {
        if (_client.Options.Headers.ContainsKey("Authorization"))
            _client.Options.Headers["Authorization"] = $"Bearer {token}";
        else
            _client.Options.Headers.Add("Authorization", $"Bearer {token}");
    }
}