using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace WarehouseAssistant.WebUI.Auth;

[UsedImplicitly]
public partial class LoginForm : ComponentBase
{
    [Inject] private IAuthService       AuthService { get; set; } = null!;
    [Inject] private ILogger<LoginForm> Logger      { get; set; } = null!;
    
    private readonly         LoginModel _loginModel = new();
    private                  bool       _loginFailed;
    [UsedImplicitly] private bool       _isBusy;
    
    private async Task HandleLogin()
    {
        _isBusy      = true;
        _loginFailed = false;
        
        Logger.LogInformation("Attempting to login to Supabase with credentials: Email={Email}, Password=***",
            _loginModel.Email);
        
        bool success = await AuthService.SignIn(_loginModel.Email, _loginModel.Password);
        
        Logger.LogInformation("Login result: {Success}", success);
        
        if (!success)
        {
            Logger.LogError("Login attempt failed for user {Email}", _loginModel.Email);
            _loginFailed = true;
        }
        
        _isBusy = false;
    }
    
    public class LoginModel
    {
        [Required(ErrorMessage = "Email не может быть пустым")]
        [EmailAddress(ErrorMessage = "Неверный формат почты")]
        public string Email { get; set; } = null!;
        
        [Required(ErrorMessage = "Пароль не может быть пустым")]
        public string Password { get; set; } = null!;
    }
}