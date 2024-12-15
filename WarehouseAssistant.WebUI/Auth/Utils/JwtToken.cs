using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace WarehouseAssistant.WebUI.Auth;

internal class JwtToken
{
    private readonly IEnumerable<Claim> _claims;
    
    public static JwtToken Parse(string jwt)
    {
        return new JwtToken(JwtParser.ParseClaimsFromJwt(jwt));
    }
    
    private JwtToken(IEnumerable<Claim> claims)
    {
        _claims = claims;
    }
    
    public AuthenticationState GetAuthenticationState()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(_claims, "jwt")));
    }
    
    public bool HasExpired()
    {
        return _claims.Any(c => c.Type == "exp");
    }
    
    public bool IsExpired()
    {
        var exp = _claims.First(c => c.Type == "exp");
        
        if (long.TryParse(exp.Value, out long expValue))
        {
            var expDate = DateTimeOffset.FromUnixTimeSeconds(expValue).UtcDateTime;
            Debug.WriteLine($"Token expiration date: {expDate}", "JwtToken");
            return expDate < DateTime.UtcNow;
        }
        
        throw new InvalidOperationException("Token does not contain an expiration date.");
    }
}