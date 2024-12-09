using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using WarehouseAssistant.WebUI.Services;

namespace WarehouseAssistant.WebUI.Pages;

public partial class LoginForm : ComponentBase
{
    private readonly LoginModel _loginModel  = new();
    private          bool       _loginFailed = false;
    private          bool       _isBusy;
    
    private async Task HandleLogin()
    {
        _isBusy      = true;
        _loginFailed = false;
        
        try
        {
            Debug.WriteLine("Authenticating user...");
            var response =
                await HttpClient.PostAsJsonAsync("https://warehouseassistantdbapi.onrender.com/api/auth/login",
                    _loginModel);
            
            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("Authentication successful. Token received.");
                    ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(token);
                }
                else
                {
                    Debug.WriteLine("Authentication failed. Token is empty.");
                    _loginFailed = true;
                }
            }
            else
            {
                Debug.WriteLine($"Authentication failed. Status code: {response.StatusCode}");
                _loginFailed = true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Authentication failed. Exception: {e.Message}");
            _loginFailed = true;
        }
        finally
        {
            Debug.WriteLine("Finished authenticating user.");
            _isBusy = false;
        }
    }
    
    public void Dispose()
    {
        _loginFailed = false;
    }
    
    public class LoginModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}