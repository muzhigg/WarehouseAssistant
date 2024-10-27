using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace WarehouseAssistant.WebUI.Services;

public class SnackbarWithSoundService(IJSRuntime jsRuntime, ISnackbar snackbarService) : IDisposable
{
    private bool TryPlaySound(Severity severity)
    {
        string soundUrl = severity switch
        {
            Severity.Normal  => string.Empty,
            Severity.Info    => "_content/WarehouseAssistant.WebUI/sounds/info_2.mp3",
            Severity.Success => "_content/WarehouseAssistant.WebUI/sounds/success.mp3",
            Severity.Warning => "_content/WarehouseAssistant.WebUI/sounds/warning.mp3",
            Severity.Error   => "_content/WarehouseAssistant.WebUI/sounds/error_2.wav",
            _                => string.Empty
        };
        
        if (string.IsNullOrEmpty(soundUrl))
            return false;
        
        jsRuntime.InvokeVoidAsync("playNotificationSound", soundUrl);
        return true;
    }
    
    public virtual Snackbar? Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        Dictionary<string, object> componentParameters = null, Severity severity = Severity.Normal,
        Action<SnackbarOptions>    configure           = null, string   key      = "") where T : IComponent
    {
        var instance = snackbarService.Add<T>(componentParameters, severity, configure, key);
        
        if (instance is not null)
            TryPlaySound(instance.Severity);
        
        return instance;
    }
    
    public virtual Snackbar? Add(string message, Severity severity = Severity.Normal,
        Action<SnackbarOptions>         configure = null,
        string                          key       = "")
    {
        var instance = snackbarService.Add(message, severity, configure, key);
        
        if (instance is not null)
            TryPlaySound(instance.Severity);
        
        return instance;
    }
    
    public virtual Snackbar? Add(RenderFragment message,          Severity severity = Severity.Normal,
        Action<SnackbarOptions>                 configure = null, string   key      = "")
    {
        var instance = snackbarService.Add(message, severity, configure, key);
        
        if (instance is not null)
            TryPlaySound(instance.Severity);
        
        return instance;
    }
    
    public virtual void Clear()
    {
        snackbarService.Clear();
    }
    
    public virtual void Remove(Snackbar snackbar)
    {
        snackbarService.Remove(snackbar);
    }
    
    public virtual void RemoveByKey(string key)
    {
        snackbarService.RemoveByKey(key);
    }
    
    public void Dispose()
    {
        snackbarService.Dispose();
    }
}