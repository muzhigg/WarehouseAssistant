namespace WarehouseAssistant.Data.Repositories;

public interface IRepository<T> where T : class
{
    bool CanWrite { get; }
    
    Task<T?>       GetByArticleAsync(string article);
    Task<List<T>?> GetAllAsync();
    Task           AddAsync(T          product);
    Task           UpdateAsync(T       product);
    Task           DeleteAsync(string? article);
}