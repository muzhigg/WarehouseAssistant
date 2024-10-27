using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;

namespace WarehouseAssistant.WebUI.Services;

public sealed class SnackbarWithSoundService(
    IJSRuntime                      jsRuntime,
    NavigationManager               navigationManager,
    IOptions<SnackbarConfiguration> configuration = null)
    : ISnackbar
{
    private readonly SnackbarService _snackbarService = new(navigationManager, configuration);
    
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
    
    public Snackbar? Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        Dictionary<string, object> componentParameters = null, Severity severity = Severity.Normal,
        Action<SnackbarOptions>    configure           = null, string   key      = "") where T : IComponent
    {
        var instance = _snackbarService.Add<T>(componentParameters, severity, configure, key);
        
        if (instance is not null)
            TryPlaySound(instance.Severity);
        
        return instance;
    }
    
    [Obsolete("Use Add instead.", true)]
    public Snackbar AddNew(Severity severity, string message, Action<SnackbarOptions> configure)
    {
        throw new NotImplementedException();
    }
    
    public Snackbar? Add(string message, Severity severity = Severity.Normal,
        Action<SnackbarOptions> configure = null,
        string                  key       = "")
    {
        var instance = _snackbarService.Add(message, severity, configure, key);
        
        if (instance is not null)
            TryPlaySound(instance.Severity);
        
        return instance;
    }
    
    public Snackbar? Add(RenderFragment message,          Severity severity = Severity.Normal,
        Action<SnackbarOptions>         configure = null, string   key      = "")
    {
        var instance = _snackbarService.Add(message, severity, configure, key);
        
        if (instance is not null)
            TryPlaySound(instance.Severity);
        
        return instance;
    }
    
    public void Clear()
    {
        _snackbarService.Clear();
    }
    
    public void Remove(Snackbar snackbar)
    {
        _snackbarService.Remove(snackbar);
    }
    
    public void RemoveByKey(string key)
    {
        _snackbarService.RemoveByKey(key);
    }
    
    public IEnumerable<Snackbar> ShownSnackbars => _snackbarService.ShownSnackbars;
    public SnackbarConfiguration Configuration  => _snackbarService.Configuration;
    
    public event Action? OnSnackbarsUpdated
    {
        add => _snackbarService.OnSnackbarsUpdated += value;
        remove => _snackbarService.OnSnackbarsUpdated -= value;
    }
    
    public void Dispose()
    {
        _snackbarService.Dispose();
    }
}