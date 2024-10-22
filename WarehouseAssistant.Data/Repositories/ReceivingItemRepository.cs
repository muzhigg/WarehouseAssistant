using System.Net.Http.Json;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Data.Repositories;

public class ReceivingItemRepository(HttpClient httpClient)
{
    private const string Uri = "https://warehouseassistantdbapi.onrender.com/api/ReceivingItem";
    
    public async Task<bool> HasActiveSessionAsync()
    {
        var response = await httpClient.GetAsync($"{Uri}/has-active-session");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<bool>();
        }
        
        return false;
    }
    
    public async Task<List<ReceivingItem>?> GetItemsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<ReceivingItem>>(Uri);
    }
    
    public async Task<bool> ReplaceItemsAsync(IEnumerable<ReceivingItem> items)
    {
        var response = await httpClient.PostAsJsonAsync($"{Uri}/replace-items", items);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> DeleteAllItemsAsync()
    {
        var response = await httpClient.DeleteAsync($"{Uri}/delete-all");
        return response.IsSuccessStatusCode;
    }
}