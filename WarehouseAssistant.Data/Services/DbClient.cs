using Microsoft.Extensions.Logging;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Shared.Models;

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
    
    public async Task DeleteRange<T>(IEnumerable<string> articles) where T : BaseModel, ITableItem, new()
    {
        await _client.Table<T>().Where(item => articles.Any(i => i == item.Article)).Delete();
    }
    
    public async Task<T?> Get<T>(string article) where T : BaseModel, ITableItem, new()
    {
        throw new NotImplementedException();
    }
    
    public async Task<List<T>> GetAll<T>() where T : BaseModel, ITableItem, new()
    {
        var get = await _client.Table<T>().Select("*").Get();
        get.ResponseMessage?.EnsureSuccessStatusCode();
        
        return get.Models;
    }
}