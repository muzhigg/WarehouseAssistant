using System.Net.Http.Json;
using WarehouseAssistant.Data.Models;

namespace WarehouseAssistant.Data.Repositories
{
    public sealed class MarketingMaterialRepository(HttpClient httpClient) : IRepository<MarketingMaterial>
    {
        private const string Uri = "https://warehouseassistantdbapi.onrender.com/api/marketingmaterials";
        
        public bool CanWrite { get; } = true;
        
        public async Task<MarketingMaterial?> GetByArticleAsync(string article)
        {
            if (string.IsNullOrEmpty(article)) return null;
            
            return await httpClient.GetFromJsonAsync<MarketingMaterial>($"{Uri}/{article}");
        }
        
        public async Task<List<MarketingMaterial>?> GetAllAsync()
        {
            return await httpClient.GetFromJsonAsync<List<MarketingMaterial>>(Uri);
        }
        
        public async Task AddAsync(MarketingMaterial marketingMaterial)
        {
            await httpClient.PostAsJsonAsync(Uri, marketingMaterial);
        }
        
        public async Task UpdateAsync(MarketingMaterial marketingMaterial)
        {
            await httpClient.PutAsJsonAsync($"{Uri}/{marketingMaterial.Article}", marketingMaterial);
        }
        
        public async Task DeleteAsync(string? article)
        {
            await httpClient.DeleteAsync($"{Uri}/{article}");
        }
        
        public Task<bool> ValidateAccessKeyAsync(string accessKey)
        {
            throw new NotImplementedException();
        }
    }
}