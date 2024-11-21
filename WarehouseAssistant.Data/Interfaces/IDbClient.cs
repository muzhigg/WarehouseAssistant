using Supabase.Postgrest.Models;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Interfaces;

public interface IDbClient
{
    void SetAuthBearer(string token);
    Task<List<T>> Get<T>(CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
    Task<T?> Get<T>(string id, CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
    
    Task<bool> Contains<T>(string id, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new();
    
    Task Insert<T>(T item, CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
    
    Task Insert<T>(ICollection<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new();
    
    Task Update<T>(T item, CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
    
    Task Update<T>(ICollection<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new();
    
    Task Upsert<T>(T item, CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
    
    Task Upsert<T>(ICollection<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new();
    
    Task Delete<T>(T item, CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
    
    Task Delete<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
        where T : BaseModel, ITableItem, new();
    
    Task Delete<T>(string id, CancellationToken cancellationToken = default) where T : BaseModel, ITableItem, new();
}