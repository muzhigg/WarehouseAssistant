using System.Net;
using System.Net.Http.Json;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.Data.Repositories
{
    public sealed class ProductRepository(HttpClient httpClient) : IRepository<Product>
    {
        private       string? _accessKey;
        private const string  Uri = "https://warehouseassistantdbapi.onrender.com/api/products";
        
        private bool? _isAuthenticated;
        
        public async Task<bool> ValidateAccessKeyAsync(string accessKey)
        {
            try
            {
                if (_isAuthenticated.HasValue)
                    return _isAuthenticated.Value;
                
                _accessKey = accessKey;
                
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{Uri}/validate-key");
                request.Headers.Add("AccessKey", accessKey);
                
                HttpResponseMessage response = await httpClient.SendAsync(request);
                
                _isAuthenticated = response.IsSuccessStatusCode;
                return _isAuthenticated.Value;
            }
            catch
            {
                return false;
            }
        }
        
        public void SetAccessKey(string accessKey)
        {
            _accessKey = accessKey;
        }
        
        public bool CanWrite => _isAuthenticated.HasValue && _isAuthenticated.Value;
        
        public async Task<Product?> GetByArticleAsync(string article)
        {
            try
            {
                return string.IsNullOrEmpty(article)
                    ? null
                    : await httpClient.GetFromJsonAsync<Product>($"{Uri}/{article}");
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
            if (_isAuthenticated == false || _accessKey == null)
                throw new HttpRequestException(HttpRequestError.UserAuthenticationError, "Access key is not valid.",
                    null,
                    HttpStatusCode.Unauthorized);
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Uri);
            request.Headers.Add("AccessKey", _accessKey);
            request.Content = JsonContent.Create(product);
            
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            _isAuthenticated = true;
        }
        
        public async Task UpdateAsync(Product product)
        {
            if (_isAuthenticated == false || _accessKey == null)
                throw new HttpRequestException(HttpRequestError.UserAuthenticationError, "Access key is not valid.",
                    null,
                    HttpStatusCode.Unauthorized);
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{Uri}/{product.Article}");
            request.Headers.Add("AccessKey", _accessKey);
            request.Content = JsonContent.Create(product);
            
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            _isAuthenticated = true;
        }
        
        public async Task DeleteAsync(string? article)
        {
            if (_isAuthenticated == false || _accessKey == null)
                throw new HttpRequestException(HttpRequestError.UserAuthenticationError, "Access key is not valid.",
                    null,
                    HttpStatusCode.Unauthorized);
            
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"{Uri}/{article}");
                request.Headers.Add("AccessKey", _accessKey);
                
                HttpResponseMessage response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                _isAuthenticated = true;
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound) { }
        }
    }
}