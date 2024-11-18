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
    
    public async Task<List<T>> Get<T>() where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> get = await _client.Table<T>().Get();
        get.ResponseMessage?.EnsureSuccessStatusCode();
        
        return get.Models;
    }
    
    public async Task<T?> Get<T>(string id) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Where(item => item.Article == id).Get();
        response.ResponseMessage?.EnsureSuccessStatusCode();
        
        return response.Model;
    }
    
    public async Task<bool> Contains<T>(string id) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response =
            await _client.Table<T>().Select("Article").Where(item => item.Article == id).Get();
        response.ResponseMessage?.EnsureSuccessStatusCode();
        
        return response.Models.Count > 0;
    }
    
    public async Task Insert<T>(T item) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Insert(item);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Insert<T>(ICollection<T> items) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Insert(items);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Update<T>(T item) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Update(item);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public Task Update<T>(ICollection<T> items) where T : BaseModel, ITableItem, new()
    {
        return Upsert(items);
    }
    
    public async Task Upsert<T>(T item) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Upsert(item, new QueryOptions()
        {
            Upsert    = true,
            Returning = QueryOptions.ReturnType.Minimal
        });
        
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Upsert<T>(ICollection<T> items) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Upsert(items, new QueryOptions()
        {
            Upsert    = true,
            Returning = QueryOptions.ReturnType.Minimal
        });
        
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Delete<T>(T item) where T : BaseModel, ITableItem, new()
    {
        ModeledResponse<T> response = await _client.Table<T>().Delete(item);
        response.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Delete<T>(IEnumerable<T> items) where T : BaseModel, ITableItem, new()
    {
        Task delete = _client.Table<T>().Filter("Article", Constants.Operator.In,
            items.Select(item => item.Article).ToList()).Delete();
        await delete;
        
        if (delete is Task<BaseResponse> deleteResponse)
            deleteResponse.Result.ResponseMessage?.EnsureSuccessStatusCode();
    }
    
    public async Task Delete<T>(string id) where T : BaseModel, ITableItem, new()
    {
        Task delete = _client.Table<T>().Where(item => item.Article == id).Delete();
        await delete;
        
        if (delete is Task<BaseResponse> deleteResponse)
            deleteResponse.Result.ResponseMessage?.EnsureSuccessStatusCode();
    }
}