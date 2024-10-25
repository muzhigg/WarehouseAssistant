using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace WarehouseAssistant.Data.Repositories;

public abstract class RepositoryBase<T>(HttpClient httpClient) : IRepository<T> where T : class
{
    protected abstract string Uri { get; }
    
    public virtual async Task DeleteRangeAsync(IEnumerable<string> articles)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Uri}/remove-range")
        {
            Content = JsonContent.Create(articles)
        };
        
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", httpClient.DefaultRequestHeaders.Authorization.Parameter);
        
        Debug.WriteLine(request.Headers.Authorization.Scheme);
        Debug.WriteLine(request.Headers.Authorization.Parameter);
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        
        // var response = await httpClient.PostAsJsonAsync($"{Uri}/remove-range", articles);
        // await httpClient.DeleteFromJsonAsync($"{Uri}/remove-range", articles);
        // response.EnsureSuccessStatusCode();
    }
    
    private bool? _isAuthenticated;
    
    [Obsolete("This property is obsolete and will be removed in a future version.")]
    public bool CanWrite => _isAuthenticated.HasValue && _isAuthenticated.Value;
    
    [Obsolete]
    public void SetAccessKey(string accessKey) { }
    
    [Obsolete]
    public async Task<bool> ValidateAccessKeyAsync(string accessKey)
    {
        try
        {
            if (_isAuthenticated.HasValue)
                return _isAuthenticated.Value;
            
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
    
    public virtual async Task<T?> GetByArticleAsync(string article)
    {
        try
        {
            return string.IsNullOrEmpty(article)
                ? null
                : await httpClient.GetFromJsonAsync<T>($"{Uri}/{article}");
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
    
    public virtual async Task<List<T>?> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<T>>($"{Uri}/all");
    }
    
    public virtual async Task AddAsync(T obj)
    {
        var response = await httpClient.PostAsJsonAsync($"{Uri}/add", obj);
        response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task AddRangeAsync(IEnumerable<T> objects)
    {
        var response =
            await httpClient.PostAsJsonAsync($"{Uri}/add-range", objects);
        response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task UpdateAsync(T obj)
    {
        var response = await httpClient.PutAsJsonAsync($"{Uri}/update", obj);
        response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task UpdateRangeAsync(IEnumerable<T> objects)
    {
        var response = await httpClient.PutAsJsonAsync($"{Uri}/update-range", objects);
        response.EnsureSuccessStatusCode();
    }
    
    public virtual async Task DeleteAsync(string? article)
    {
        var response = await httpClient.DeleteAsync($"{Uri}/remove/{article}");
        response.EnsureSuccessStatusCode();
    }
}