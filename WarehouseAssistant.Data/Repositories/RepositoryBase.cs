using Supabase.Postgrest.Models;
using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Repositories;

public abstract class RepositoryBase<T>(IDbClient client) : IRepository<T> where T : BaseModel, ITableItem, new()
{
    private bool? _isAuthenticated;
    
    [Obsolete("This property is obsolete and will be removed in a future version.")]
    public bool CanWrite => _isAuthenticated.HasValue && _isAuthenticated.Value;
    
    [Obsolete]
    public void SetAccessKey(string accessKey) { }
    
    public virtual async Task DeleteRangeAsync(IEnumerable<T> objects)
    {
        await client.Delete<T>(objects);
    }
    
    [Obsolete]
    public async Task<bool> ValidateAccessKeyAsync(string accessKey)
    {
        return true;
    }
    
    public virtual async Task<T?> GetByArticleAsync(string article)
    {
        return await client.Get<T>(article);
    }
    
    public virtual async Task<List<T>?> GetAllAsync()
    {
        return await client.Get<T>();
    }
    
    public virtual async Task<bool> ContainsAsync(string article)
    {
        return await client.Contains<T>(article);
    }
    
    public virtual async Task AddAsync(T obj)
    {
        await client.Insert(obj);
    }
    
    public virtual async Task AddRangeAsync(ICollection<T> objects)
    {
        await client.Insert(objects);
    }
    
    public virtual async Task UpdateAsync(T obj)
    {
        await client.Update(obj);
    }
    
    public virtual async Task UpdateRangeAsync(ICollection<T> objects)
    {
        await client.Update(objects);
    }
    
    public virtual async Task DeleteAsync(string? article)
    {
        await client.Delete<T>(article);
    }
    
    public virtual async Task DeleteAsync(T obj)
    {
        await client.Delete<T>(obj);
    }
}