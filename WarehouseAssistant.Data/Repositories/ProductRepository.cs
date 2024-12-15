using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.Data.Repositories;

public sealed class ProductRepository(IDbClient client) : RepositoryBase<Product>(client)
{
    protected string Uri => "https://warehouseassistantdbapi.onrender.com/api/products";
}