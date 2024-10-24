namespace WarehouseAssistant.Data.Repositories;

public interface IRepository<T> where T : class
{
    [Obsolete("This property is deprecated and will be removed in a future version.")]
    public bool CanWrite { get; }
    
    Task<T?>       GetByArticleAsync(string article);
    Task<List<T>?> GetAllAsync();
    Task           AddAsync(T                           obj);
    Task           AddRangeAsync(IEnumerable<T>         objects);
    Task           UpdateAsync(T                        obj);
    Task           UpdateRangeAsync(IEnumerable<T>      objects);
    Task           DeleteAsync(string                   article);
    Task           DeleteRangeAsync(IEnumerable<string> articles);
    
    [Obsolete("This method is deprecated and will be removed in a future version.")]
    Task<bool> ValidateAccessKeyAsync(string accessKey);
}