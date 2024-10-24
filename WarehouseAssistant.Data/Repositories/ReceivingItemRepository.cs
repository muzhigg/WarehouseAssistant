using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Repositories;

public class ReceivingItemRepository(HttpClient httpClient) : RepositoryBase<ReceivingItem>(httpClient)
{
    protected override string Uri => "https://warehouseassistantdbapi.onrender.com/api/receivingitem";
}