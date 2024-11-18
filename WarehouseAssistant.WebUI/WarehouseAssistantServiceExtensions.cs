using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using WarehouseAssistant.Data.Interfaces;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Data.Services;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Auth;
using WarehouseAssistant.WebUI.DatabaseModule;
using WarehouseAssistant.WebUI.Services;

namespace WarehouseAssistant.WebUI
{
    public static class WarehouseAssistantServiceExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddWebUIServices(this IServiceCollection services)
        {
            AddMudBlazorServices(services, config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
                
                config.SnackbarConfiguration.PreventDuplicates      = false;
                config.SnackbarConfiguration.NewestOnTop            = false;
                config.SnackbarConfiguration.ShowCloseIcon          = true;
                config.SnackbarConfiguration.VisibleStateDuration   = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant        = Variant.Filled;
            });
            AddLocalStorage(services);
            AddAuthServices(services);
            AddSupabaseServices(services);
            AddDataServices(services);
            services.AddScoped<SnackbarWithSoundService>();
            
            return services;
        }
        
        private static void AddSupabaseServices(IServiceCollection services)
        {
            string apiKey =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InV0eWlneXJvaHd3cndwcnByZ2J1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mjk1NzYyNjMsImV4cCI6MjA0NTE1MjI2M30.cWEf0OZtYrcu6UHe_ewPB5eC53QbE0rupRO-VaZJUiQ";
            string endpoint = "https://utyigyrohwwrwprprgbu.supabase.co";
            
            services.AddSingleton<IGotrueClient<User, Session>>(provider =>
            {
                var client = new Client(new ClientOptions()
                {
                    AllowUnconfirmedUserSessions = true,
                    AutoRefreshToken             = true,
                    Url                          = $"{endpoint}/auth/v1",
                    Headers = new Dictionary<string, string>()
                    {
                        {
                            "apiKey", apiKey
                        }
                    }
                });
                var logger = provider.GetRequiredService<ILogger<Client>>();
                client.AddDebugListener((s, exception) => logger.LogError(exception, s));
                return client;
            });
            
            services.AddSingleton<IDbClient, DbClient>(provider =>
            {
                var client =
                    new DbClient($"{endpoint}/rest/v1",
                        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InV0eWlneXJvaHd3cndwcnByZ2J1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mjk1NzYyNjMsImV4cCI6MjA0NTE1MjI2M30.cWEf0OZtYrcu6UHe_ewPB5eC53QbE0rupRO-VaZJUiQ",
                        provider.GetRequiredService<ILogger<DbClient>>());
                
                var authClient = provider.GetRequiredService<IGotrueClient<User, Session>>();
                authClient.AddStateChangedListener((sender, changed) =>
                {
                    switch (changed)
                    {
                        case Constants.AuthState.SignedIn:
                        case Constants.AuthState.UserUpdated:
                        case Constants.AuthState.TokenRefreshed:
                            client.SetAuthBearer(sender.CurrentSession.AccessToken);
                            break;
                        case Constants.AuthState.SignedOut:
                            client.SetAuthBearer("");
                            break;
                    }
                });
                
                if (authClient.CurrentSession != null)
                {
                    client.SetAuthBearer(authClient.CurrentSession.AccessToken!);
                }
                
                return client;
            });
        }
        
        private static void AddDataServices(IServiceCollection services)
        {
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<IRepository<ReceivingItem>, ReceivingItemRepository>();
            services.AddScoped<IProductFormDialogService, ProductFormDialogService>();
        }
        
        private static void AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<AuthenticationStateProvider>((provider =>
            {
                if (provider.GetRequiredService<IAuthService>() is CustomAuthenticationStateProvider s) return s;
                
                throw new InvalidOperationException("Authentication state provider not found");
            }));
            services.AddScoped<IAuthService, CustomAuthenticationStateProvider>();
            services.AddAuthorizationCore();
        }
        
        private static void AddMudBlazorServices(IServiceCollection services,
            Action<MudServicesConfiguration>                        configuration)
        {
            var options = new MudServicesConfiguration();
            configuration(options);
            
            services.AddMudBlazorDialog()
                .AddSnackbar(snackBarConfiguration =>
                {
                    snackBarConfiguration.ClearAfterNavigation   = options.SnackbarConfiguration.ClearAfterNavigation;
                    snackBarConfiguration.MaxDisplayedSnackbars  = options.SnackbarConfiguration.MaxDisplayedSnackbars;
                    snackBarConfiguration.NewestOnTop            = options.SnackbarConfiguration.NewestOnTop;
                    snackBarConfiguration.PositionClass          = options.SnackbarConfiguration.PositionClass;
                    snackBarConfiguration.PreventDuplicates      = options.SnackbarConfiguration.PreventDuplicates;
                    snackBarConfiguration.MaximumOpacity         = options.SnackbarConfiguration.MaximumOpacity;
                    snackBarConfiguration.ShowTransitionDuration = options.SnackbarConfiguration.ShowTransitionDuration;
                    snackBarConfiguration.VisibleStateDuration   = options.SnackbarConfiguration.VisibleStateDuration;
                    snackBarConfiguration.HideTransitionDuration = options.SnackbarConfiguration.HideTransitionDuration;
                    snackBarConfiguration.ShowCloseIcon          = options.SnackbarConfiguration.ShowCloseIcon;
                    snackBarConfiguration.RequireInteraction     = options.SnackbarConfiguration.RequireInteraction;
                    snackBarConfiguration.BackgroundBlurred      = options.SnackbarConfiguration.BackgroundBlurred;
                    snackBarConfiguration.SnackbarVariant        = options.SnackbarConfiguration.SnackbarVariant;
                }).AddMudBlazorResizeListener(resizeOptions =>
                {
                    resizeOptions.BreakpointDefinitions  = options.ResizeOptions.BreakpointDefinitions;
                    resizeOptions.EnableLogging          = options.ResizeOptions.EnableLogging;
                    resizeOptions.NotifyOnBreakpointOnly = options.ResizeOptions.NotifyOnBreakpointOnly;
                    resizeOptions.ReportRate             = options.ResizeOptions.ReportRate;
                    resizeOptions.SuppressInitEvent      = options.ResizeOptions.SuppressInitEvent;
                })
                .AddMudBlazorResizeObserver(observerOptions =>
                {
                    observerOptions.EnableLogging = options.ResizeObserverOptions.EnableLogging;
                    observerOptions.ReportRate    = options.ResizeObserverOptions.ReportRate;
                })
                .AddMudBlazorResizeObserverFactory(observerOptions =>
                {
                    observerOptions.EnableLogging = options.ResizeObserverOptions.EnableLogging;
                    observerOptions.ReportRate    = options.ResizeObserverOptions.ReportRate;
                })
                .AddMudBlazorKeyInterceptor()
                .AddMudBlazorJsEvent()
                .AddMudBlazorScrollManager()
                .AddMudBlazorScrollListener()
                .AddMudBlazorJsApi()
                .AddMudPopoverService(popoverOptions =>
                {
                    popoverOptions.ContainerClass           = options.PopoverOptions.ContainerClass;
                    popoverOptions.FlipMargin               = options.PopoverOptions.FlipMargin;
                    popoverOptions.QueueDelay               = options.PopoverOptions.QueueDelay;
                    popoverOptions.ThrowOnDuplicateProvider = options.PopoverOptions.ThrowOnDuplicateProvider;
                    popoverOptions.Mode                     = options.PopoverOptions.Mode;
                    popoverOptions.PoolSize                 = options.PopoverOptions.PoolSize;
                    popoverOptions.PoolInitialFill          = options.PopoverOptions.PoolInitialFill;
                })
                .AddMudBlazorScrollSpy()
                .AddMudEventManager()
                .AddMudLocalization();
        }
        
        private static IServiceCollection AddSnackbar(this IServiceCollection services,
            Action<SnackbarConfiguration>                                     options)
        {
            services.AddScoped<ISnackbar, SnackbarWithSoundService>();
            services.Configure(options);
            
            return services;
        }
        
        private static void AddLocalStorage(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage(cfg => { cfg.JsonSerializerOptions.IgnoreReadOnlyProperties = true; });
        }
    }
}