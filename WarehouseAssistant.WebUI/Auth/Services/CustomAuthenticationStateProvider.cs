using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace WarehouseAssistant.WebUI.Auth;

public class CustomAuthenticationStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
    : AuthenticationStateProvider
{
    private const string LocalStorageKey = "authToken";
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorageService.GetItemAsync<string>(LocalStorageKey);
        Debug.WriteLine("Retrieved token from local storage.", nameof(CustomAuthenticationStateProvider));
        
        if (string.IsNullOrEmpty(token))
        {
            Debug.WriteLine("Token is null or empty.", nameof(CustomAuthenticationStateProvider));
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        var decodedToken = JwtToken.Parse(token);
        Debug.WriteLine("Parsed claims from JWT.", nameof(CustomAuthenticationStateProvider));
        
        if (decodedToken.HasExpired() == false
            || decodedToken.IsExpired())
        {
            Debug.WriteLine("Token expired.", nameof(CustomAuthenticationStateProvider));
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Debug.WriteLine("User authenticated and HTTP client authorization header set.",
            nameof(CustomAuthenticationStateProvider));
        
        return decodedToken.GetAuthenticationState();
    }
    
    internal void MarkUserAsAuthenticated(string token)
    {
        JwtToken jwtToken = JwtToken.Parse(token);
        Debug.WriteLine("User authenticated and JWT token parsed.", nameof(CustomAuthenticationStateProvider));
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Debug.WriteLine("HTTP client authorization header set.", nameof(CustomAuthenticationStateProvider));
        
        NotifyAuthenticationStateChanged(Task.FromResult(jwtToken.GetAuthenticationState()));
        localStorageService.SetItemAsync(LocalStorageKey, token);
    }
    
    internal void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        httpClient.DefaultRequestHeaders.Authorization = null;
        localStorageService.RemoveItemAsync(LocalStorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }
}