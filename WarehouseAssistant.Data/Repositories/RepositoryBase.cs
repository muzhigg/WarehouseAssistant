using Supabase.Postgrest.Models;
using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Repositories;

public abstract class RepositoryBase<T>(IDbClient client) : IRepository<T> where T : BaseModel, ITableItem, new()
{
    [Obsolete] private bool? _isAuthenticated;
    
    [Obsolete("This property is obsolete and will be removed in a future version.")]
    public bool CanWrite => _isAuthenticated.HasValue && _isAuthenticated.Value;
    
    public virtual async Task DeleteRangeAsync(IEnumerable<T> objects,
        CancellationToken                                     cancellationToken = default)
    {
        await client.Delete(objects, cancellationToken);
    }
    
    [Obsolete]
    public Task<bool> ValidateAccessKeyAsync(string accessKey)
    {
        return Task.FromResult(true);
    }
    
    public virtual async Task<T?> GetByArticleAsync(string article,
        CancellationToken                                  cancellationToken = default)
    {
        return await client.Get<T>(article, cancellationToken);
    }
    
    public virtual async Task<List<T>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await client.Get<T>(cancellationToken);
    }
    
    public virtual async Task<bool> ContainsAsync(string article,
        CancellationToken                                cancellationToken = default)
    {
        return await client.Contains<T>(article, cancellationToken);
    }
    
    public virtual async Task AddAsync(T obj,
        CancellationToken                cancellationToken = default)
    {
        await client.Insert(obj, cancellationToken);
    }
    
    public virtual async Task AddRangeAsync(ICollection<T> objects,
        CancellationToken                                  cancellationToken = default)
    {
        await client.Insert(objects, cancellationToken);
    }
    
    public virtual async Task UpdateAsync(T obj,
        CancellationToken                   cancellationToken = default)
    {
        await client.Update(obj, cancellationToken);
    }
    
    public virtual async Task UpdateRangeAsync(ICollection<T> objects,
        CancellationToken                                     cancellationToken = default)
    {
        await client.Update(objects, cancellationToken);
    }
    
    public virtual async Task DeleteAsync(string article,
        CancellationToken                        cancellationToken = default)
    {
        await client.Delete<T>(article, cancellationToken);
    }
    
    public virtual async Task DeleteAsync(T obj,
        CancellationToken                   cancellationToken = default)
    {
        await client.Delete(obj, cancellationToken);
    }
}