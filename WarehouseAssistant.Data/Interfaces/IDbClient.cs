using Supabase.Postgrest.Models;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Interfaces;

public interface IDbClient
{
    void          SetAuthBearer(string               token);
    Task          DeleteRange<T>(IEnumerable<string> articles) where T : BaseModel, ITableItem, new();
    Task<List<T>> GetAll<T>() where T : BaseModel, ITableItem, new();
}