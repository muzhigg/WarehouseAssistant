using Supabase.Postgrest.Models;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Interfaces;

public interface IDbClient
{
    void          SetAuthBearer(string token);
    Task<List<T>> Get<T>() where T : BaseModel, ITableItem, new();
    Task<T?>      Get<T>(string            id) where T : BaseModel, ITableItem, new();
    Task<bool>    Contains<T>(string       id) where T : BaseModel, ITableItem, new();
    Task          Insert<T>(T              item) where T : BaseModel, ITableItem, new();
    Task          Insert<T>(ICollection<T> items) where T : BaseModel, ITableItem, new();
    Task          Update<T>(T              item) where T : BaseModel, ITableItem, new();
    Task          Update<T>(ICollection<T> items) where T : BaseModel, ITableItem, new();
    Task          Upsert<T>(T              item) where T : BaseModel, ITableItem, new();
    Task          Upsert<T>(ICollection<T> items) where T : BaseModel, ITableItem, new();
    Task          Delete<T>(T              item) where T : BaseModel, ITableItem, new();
    Task          Delete<T>(IEnumerable<T> items) where T : BaseModel, ITableItem, new();
    Task          Delete<T>(string         id) where T : BaseModel, ITableItem, new();
}