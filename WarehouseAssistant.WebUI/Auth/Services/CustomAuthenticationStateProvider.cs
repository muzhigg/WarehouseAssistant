using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;

namespace WarehouseAssistant.WebUI.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IAuthService
{
    private const string LocalStorageKey = "user_session";
    
    private readonly ILocalStorageService _localStorageService;
    
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    
    private readonly IGotrueClient<User, Session> _client;
    
    public CustomAuthenticationStateProvider(ILocalStorageService localStorageService,
        ILogger<CustomAuthenticationStateProvider>                logger, IGotrueClient<User, Session> client)
    {
        _localStorageService = localStorageService;
        _logger              = logger;
        _client              = client;
        _client.AddStateChangedListener(AuthEventHandler);
    }
    
    private void AuthEventHandler(IGotrueClient<User, Session> sender, Constants.AuthState statechanged)
    {
        _logger.LogInformation("Received auth event: {AuthState}", statechanged);
        
        switch (statechanged)
        {
            case Constants.AuthState.SignedOut:
            {
                // Fixing Supabase client bug
                // When from hibernation exit token is not updated
                if (_client.CurrentSession != null)
                {
                    Session? session = _client.CurrentSession;
                    if (!string.IsNullOrEmpty(session.AccessToken) && !string.IsNullOrEmpty(session.RefreshToken))
                        _client.SetSession(session.AccessToken, session.RefreshToken);
                    else
                        _ = SignOut();
                }
                
                break;
            }
            case Constants.AuthState.TokenRefreshed:
            {
                _logger.LogInformation("Token refreshed, saving session to local storage");
                _localStorageService.SetItemAsync(LocalStorageKey,
                        sender.CurrentSession)
                    .AndForget(true);
                
                // Parse the JWT access token
                JwtToken jwtToken =
                    JwtToken.Parse(sender.CurrentSession!.AccessToken!);
                
                // Notify the authentication state has changed
                NotifyAuthenticationStateChanged(Task.FromResult(jwtToken.GetAuthenticationState()));
                break;
            }
        }
    }
    
    /// <inheritdoc />
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _logger.LogInformation("Attempting to get authentication state...");
        
        // Attempt to load the user session from local storage
        Session? session = await _localStorageService.GetItemAsync<Session>(LocalStorageKey);
        
        // If there is no session, return an empty authentication state
        if (session == null)
        {
            _logger.LogInformation("No session found in local storage, returning empty authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        // If we have a session, attempt to refresh the access token
        if (session is { AccessToken: not null, RefreshToken: not null })
        {
            try
            {
                _logger.LogInformation("Attempting to set access token...");
                
                // Set the session and get the new one
                Session  newSession   = await _client.SetSession(session.AccessToken, session.RefreshToken);
                JwtToken decodedToken = JwtToken.Parse(newSession.AccessToken);
                
                // If the new session has an access token, parse the JWT and return the authentication state
                if (newSession.AccessToken != session.AccessToken)
                {
                    _logger.LogInformation("Access token refreshed, parsing JWT...");
                    
                    if (session.CreatedAt != newSession.CreatedAt)
                    {
                        _logger.LogInformation("Session has been refreshed, updating local storage.");
                        await _localStorageService.SetItemAsync(LocalStorageKey, newSession);
                    }
                    
                    return decodedToken.GetAuthenticationState();
                }
                
                return decodedToken.GetAuthenticationState();
            }
            catch (GotrueException exception)
            {
                _logger.LogError(exception, "Failed to refresh access token, returning empty authentication state");
            }
        }
        
        // If we couldn't refresh the session, return an empty authentication state
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
    
    /// <inheritdoc />
    public async Task<bool> SignIn(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting to sign in user with email: {Email}", email);
            
            // Attempt to sign the user in
            Session? session = await _client.SignIn(email, password);
            
            // If the sign in was successful, notify the authentication state has changed
            if (session != null)
            {
                _logger.LogInformation("Sign in successful for user with email: {Email}", email);
                
                // Parse the JWT access token
                JwtToken jwtToken =
                    JwtToken.Parse(session.AccessToken ?? throw new NullReferenceException("Access token is null."));
                
                // Notify the authentication state has changed
                NotifyAuthenticationStateChanged(Task.FromResult(jwtToken.GetAuthenticationState()));
                
                // Save the user session to local storage
                await _localStorageService.SetItemAsync(LocalStorageKey, session);
                _logger.LogInformation("User session saved to local storage for email: {Email}", email);
                
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Sign in failed for user with email: {Email}", email);
            // If there was an error, return false
            return false;
        }
        
        _logger.LogWarning("Sign in failed for user with email: {Email}, session was null", email);
        // Return false if the user was not signed in
        return false;
    }
    
    public async Task SignOut()
    {
        _logger.LogInformation("Attempting to sign out...");
        try
        {
            await _client.SignOut();
            _logger.LogInformation("Sign out successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during sign out.");
        }
        finally
        {
            await _localStorageService.RemoveItemAsync(LocalStorageKey);
            _logger.LogInformation("User session removed from local storage.");
            
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
            _logger.LogInformation("Authentication state changed to anonymous.");
        }
    }
}