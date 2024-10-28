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
        Debug.WriteLine("Retrieved token from local storage.");
        
        if (string.IsNullOrEmpty(token))
        {
            Debug.WriteLine("Token is null or empty.");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        var claims = JwtParser.ParseClaimsFromJwt(token);
        Debug.WriteLine("Parsed claims from JWT.");
        
        // ReSharper disable once PossibleMultipleEnumeration
        var expiryClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expiryClaim != null && long.TryParse(expiryClaim.Value, out var exp))
        {
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            Debug.WriteLine("Token expiry date: " + expiryDate);
            
            if (expiryDate < DateTime.UtcNow)
            {
                Debug.WriteLine("Token has expired.");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        
        // ReSharper disable once PossibleMultipleEnumeration
        var identity = new ClaimsIdentity(claims, "jwt");
        var user     = new ClaimsPrincipal(identity);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Debug.WriteLine("User authenticated and HTTP client authorization header set.");
        
        return new AuthenticationState(user);
    }
    
    internal void MarkUserAsAuthenticated(string token)
    {
        var claims   = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        
        var user = new ClaimsPrincipal(identity);
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
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