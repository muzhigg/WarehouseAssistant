﻿using Microsoft.Extensions.Logging;
using WarehouseAssistant.WebUI;

namespace WarehouseAssistant.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });
            
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped(_ => new HttpClient());
            builder.Services.AddWebUIServices();
            builder.Services.AddLogging();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}