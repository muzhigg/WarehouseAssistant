using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace WarehouseAssistant.WebUI.Services;

public class CustomAuthenticationStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
    : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Получаем токен из локального хранилища
        var token = await localStorageService.GetItemAsync<string>("authToken");
        
        if (string.IsNullOrEmpty(token))
            // Пользователь не авторизован
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        
        // Проверяем токен и извлекаем claims
        var claims   = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
    
    public void MarkUserAsAuthenticated(string token)
    {
        var claims   = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        
        var user = new ClaimsPrincipal(identity);
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        localStorageService.SetItemAsync("authToken", token);
    }
    
    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }
}