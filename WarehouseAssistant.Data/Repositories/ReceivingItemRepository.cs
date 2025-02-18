using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Repositories;

public class ReceivingItemRepository(IDbClient client) : RepositoryBase<ReceivingItem>(client)
{
    protected virtual string Uri => "https://warehouseassistantdbapi.onrender.com/api/receivingitem";
}