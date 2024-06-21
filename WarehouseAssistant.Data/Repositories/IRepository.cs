using WarehouseAssistant.Data.Models;

namespace WarehouseAssistant.Data.Repositories;

public interface IRepository<T> where T : class
{
    Task<T>              GetByArticleAsync(string article);
    Task<IEnumerable<T>> GetAllAsync();
    Task                       AddAsync(T    product);
    Task                       UpdateAsync(T product);
    Task                       DeleteAsync(string     article);
}