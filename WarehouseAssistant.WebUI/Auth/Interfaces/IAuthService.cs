namespace WarehouseAssistant.WebUI.Auth;

public interface IAuthService
{
    /// <summary>
    /// Signs the user in with their email and password.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>True if the user was signed in successfully, false otherwise.</returns>
    Task<bool> SignIn(string email, string password);
    
    Task SignOut();
}