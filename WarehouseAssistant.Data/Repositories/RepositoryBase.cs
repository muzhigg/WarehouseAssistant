using Supabase.Postgrest.Models;
using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Repositories;

public abstract class RepositoryBase<T>(IDbClient client) : IRepository<T> where T : BaseModel, ITableItem, new()
{
    protected abstract string Uri { get; }
    
    public virtual async Task DeleteRangeAsync(IEnumerable<string> articles)
    {
        await client.DeleteRange<T>(articles);
    }
    
    private bool? _isAuthenticated;
    
    [Obsolete("This property is obsolete and will be removed in a future version.")]
    public bool CanWrite => _isAuthenticated.HasValue && _isAuthenticated.Value;
    
    [Obsolete]
    public void SetAccessKey(string accessKey) { }
    
    [Obsolete]
    public async Task<bool> ValidateAccessKeyAsync(string accessKey)
    {
        return true;
    }
    
    public virtual async Task<T?> GetByArticleAsync(string article)
    {
        throw new NotImplementedException();
    }
    
    public virtual async Task<List<T>?> GetAllAsync()
    {
        return await client.GetAll<T>();
    }
    
    public virtual async Task AddAsync(T obj)
    {
        // var response = await httpClient.PostAsJsonAsync($"{Uri}/add", obj);
        // response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task AddRangeAsync(IEnumerable<T> objects)
    {
        // var response =
        //     await httpClient.PostAsJsonAsync($"{Uri}/add-range", objects);
        // response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task UpdateAsync(T obj)
    {
        // var response = await httpClient.PutAsJsonAsync($"{Uri}/update", obj);
        // response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task UpdateRangeAsync(IEnumerable<T> objects)
    {
        // var response = await httpClient.PutAsJsonAsync($"{Uri}/update-range", objects);
        // response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task DeleteAsync(string? article)
    {
        // var response = await httpClient.DeleteAsync($"{Uri}/remove/{article}");
        // response.EnsureSuccessStatusCode();
    }
}