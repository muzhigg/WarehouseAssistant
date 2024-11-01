using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;

namespace WarehouseAssistant.WebUI.Auth;

[ExcludeFromCodeCoverage]
internal static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var                         payload       = jwt.Split('.')[1];
        byte[]                      jsonBytes     = ParseBase64WithoutPadding(payload);
        Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        if (keyValuePairs != null)
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
        
        return [];
    }
    
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }
        
        return Convert.FromBase64String(base64);
    }
}