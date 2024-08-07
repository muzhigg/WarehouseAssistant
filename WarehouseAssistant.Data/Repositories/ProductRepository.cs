using System.Net;
using System.Net.Http.Json;
using WarehouseAssistant.Data.Models;

namespace WarehouseAssistant.Data.Repositories
{
    public sealed class ProductRepository(HttpClient httpClient) : IRepository<Product>
    {
        private const string Uri = "https://warehouseassistantdbapi.onrender.com/api/products";

        public async Task<Product?> GetByArticleAsync(string article)
        {
            try
            {
                return string.IsNullOrEmpty(article) ? null : await httpClient.GetFromJsonAsync<Product>($"{Uri}/{article}");
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<List<Product>?> GetAllAsync()
        {
            return await httpClient.GetFromJsonAsync<List<Product>>(Uri);
        }

        public async Task AddAsync(Product product)
        {
            await httpClient.PostAsJsonAsync(Uri, product);
        }

        public async Task UpdateAsync(Product product)
        {
            await httpClient.PutAsJsonAsync($"{Uri}/{product.Article}", product);
        }

        public async Task DeleteAsync(string? article)
        {
            try
            {
                await httpClient.DeleteAsync($"{Uri}/{article}");
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound) { }
        }
    }
}
