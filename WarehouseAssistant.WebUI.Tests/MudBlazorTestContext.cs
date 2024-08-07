using MudBlazor;
using MudBlazor.Services;
using System.Net.Http;

namespace WarehouseAssistant.WebUI.Tests;

public class MudBlazorTestContext : TestContext
{
    public MudBlazorTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddScoped(_ => new HttpClient());
        Services.AddOptions();
    }

    protected IRenderedComponent<MudDialogProvider> RenderedDialogProvider(out DialogService? service)
    {
        IRenderedComponent<MudDialogProvider> provider = RenderComponent<MudDialogProvider>();
        service = Services.GetService<IDialogService>() as DialogService;
        return provider;
    }
}