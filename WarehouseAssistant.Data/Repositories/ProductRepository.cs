using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.Data.Repositories;

public sealed class ProductRepository(HttpClient httpClient) : RepositoryBase<Product>(httpClient)
{
    protected override string Uri => "https://warehouseassistantdbapi.onrender.com/api/products";
}