using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace WarehouseAssistant.WebUI.Tests;

public class MudBlazorTestContext : TestContext
{
    private Mock<AuthenticationStateProvider> _authenticationStateProvider = new Mock<AuthenticationStateProvider>();
    
    protected MudBlazorTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        // Services.AddMudServices();
        Services.AddScoped(_ => new HttpClient());
        Services.AddOptions();
        Services.AddMudBlazorResizeListener()
            .AddMudBlazorResizeObserver()
            .AddMudBlazorResizeObserverFactory()
            .AddMudBlazorKeyInterceptor()
            .AddMudBlazorJsEvent()
            .AddMudBlazorScrollManager()
            .AddMudBlazorScrollListener()
            .AddMudBlazorJsApi()
            .AddMudBlazorScrollSpy()
            .AddMudPopoverService()
            .AddMudEventManager()
            .AddMudLocalization();
        Services.AddSingleton(_authenticationStateProvider.Object);
        
        _authenticationStateProvider.Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "jwt"))));
    }
    
    protected IRenderedComponent<MudDialogProvider> RenderedDialogProvider(out DialogService? service)
    {
        IRenderedComponent<MudDialogProvider> provider = RenderComponent<MudDialogProvider>();
        service = Services.GetService<IDialogService>() as DialogService;
        return provider;
    }
    
    protected IRenderedComponent<MudSnackbarProvider> RenderedSnackbarProvider(out SnackbarService? service)
    {
        IRenderedComponent<MudSnackbarProvider> provider = RenderComponent<MudSnackbarProvider>();
        service = Services.GetService<ISnackbar>() as SnackbarService;
        return provider;
    }
}