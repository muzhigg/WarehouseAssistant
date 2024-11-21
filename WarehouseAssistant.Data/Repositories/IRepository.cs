using Supabase.Postgrest.Models;

namespace WarehouseAssistant.Data.Repositories;

public interface IRepository<T> where T : BaseModel
{
    [Obsolete("This property is deprecated and will be removed in a future version.")]
    public bool CanWrite { get; }
    
    Task<T?>       GetByArticleAsync(string        article, CancellationToken cancellationToken = default);
    Task<List<T>?> GetAllAsync(CancellationToken   cancellationToken                            = default);
    Task           AddAsync(T                      obj,     CancellationToken cancellationToken = default);
    Task           AddRangeAsync(ICollection<T>    objects, CancellationToken cancellationToken = default);
    Task           UpdateAsync(T                   obj,     CancellationToken cancellationToken = default);
    Task           UpdateRangeAsync(ICollection<T> objects, CancellationToken cancellationToken = default);
    Task           DeleteAsync(string              article, CancellationToken cancellationToken = default);
    Task           DeleteAsync(T                   obj,     CancellationToken cancellationToken = default);
    Task           DeleteRangeAsync(IEnumerable<T> objects, CancellationToken cancellationToken = default);
    Task<bool>     ContainsAsync(string            article, CancellationToken cancellationToken = default);
}