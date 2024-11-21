using Microsoft.Extensions.Logging;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Responses;
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
                _logger.LogInformation(message);
        });
    }
    
    public void SetAuthBearer(string token)
    {
        if (_client.Options.Headers.ContainsKey("Authorization"))
            _client.Options.Headers["Authorization"] = $"Bearer {token}";
        else
            _client.Options.Headers.Add("Authorization", $"Bearer {token}");
    }
    
    public async Task<List<T>> Get<T>(CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> get = await _client.Table<T>().Get(cancellationToken);
        
        get.ResponseMessage?.EnsureSuccessStatusCode();
        
        return get.Models;
    }
    
    public async Task<T?> Get<T>(string id, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Where(item => item.Article == id).Get(cancellationToken);
        response.ResponseMessage?.EnsureSuccessStatusCode();
        
        return response.Model;
    }
    
    public async Task<bool> Contains<T>(string id, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response =
            await _client.Table<T>().Select("Article").Where(item => item.Article == id).Get(cancellationToken);
        response.ResponseMessage?.EnsureSuccessStatusCode();
        
        return response.Models.Count > 0;
    }
    
    public async Task Insert<T>(T item, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Insert(item, cancellationToken: cancellationToken);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Insert<T>(ICollection<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Insert(items, cancellationToken: cancellationToken);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Update<T>(T item, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Update(item, cancellationToken: cancellationToken);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public Task Update<T>(ICollection<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        return Upsert(items, cancellationToken);
    }
    
    public async Task Upsert<T>(T item, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Upsert(item, new QueryOptions()
        {
            Upsert    = true,
            Returning = QueryOptions.ReturnType.Minimal
        }, cancellationToken);
        
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Upsert<T>(ICollection<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Upsert(items, new QueryOptions()
        {
            Upsert    = true,
            Returning = QueryOptions.ReturnType.Minimal
        }, cancellationToken);
        
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Delete<T>(T item, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Delete(item, cancellationToken: cancellationToken);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Delete<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        Task delete = _client.Table<T>().Filter("Article", Constants.Operator.In,
            items.Select(item => item.Article).ToList()).Delete(cancellationToken: cancellationToken);
        await delete;
        
        if (delete is Task<BaseResponse> deleteResponse)
            deleteResponse.Result.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Delete<T>(string id, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new()
    {
        Task delete = _client.Table<T>().Where(item => item.Article == id).Delete(cancellationToken: cancellationToken);
        await delete;
        
        if (delete is Task<BaseResponse> deleteResponse)
            deleteResponse.Result.ResponseMessage?.EnsureSuccessStatusCode();
    }
}