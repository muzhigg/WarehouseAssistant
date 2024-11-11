using System;
using System.Linq.Expressions;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace WarehouseAssistant.WebUI.Tests;

public class MudBlazorTestContext : TestContext
{
    protected MudBlazorTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
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
    
    protected void VerifyLogInfo<T>(Mock<ILogger<T>> loggerMock,
        Expression<Func<string, bool>>               match,
        Times                                        times)
    {
        loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => match.Compile().Invoke(o.ToString()!)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
    
    protected void VerifyLogError<T>(Mock<ILogger<T>> loggerMock,
        Times                                         times)
    {
        loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
    
    protected void VerifyLogError<T>(Mock<ILogger<T>> loggerMock,
        Expression<Func<string, bool>>                match,
        Times                                         times)
    {
        loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => match.Compile().Invoke(o.ToString()!)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}