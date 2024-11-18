using Supabase.Postgrest.Models;

namespace WarehouseAssistant.Data.Repositories;

public interface IRepository<T> where T : BaseModel
{
    [Obsolete("This property is deprecated and will be removed in a future version.")]
    public bool CanWrite { get; }
    
    Task<T?>       GetByArticleAsync(string article);
    Task<List<T>?> GetAllAsync();
    Task           AddAsync(T                      obj);
    Task           AddRangeAsync(ICollection<T>    objects);
    Task           UpdateAsync(T                   obj);
    Task           UpdateRangeAsync(ICollection<T> objects);
    Task           DeleteAsync(string              article);
    Task           DeleteAsync(T                   obj);
    Task           DeleteRangeAsync(IEnumerable<T> objects);
    
    [Obsolete("This method is deprecated and will be removed in a future version.")]
    Task<bool> ValidateAccessKeyAsync(string accessKey);
}