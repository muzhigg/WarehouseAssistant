using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
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
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<IRepository<ReceivingItem>, ReceivingItemRepository>();
            services.AddScoped<SnackbarWithSoundService>();
            services.AddScoped<IProductFormDialogService, ProductFormDialogService>();
            return services;
        }
        
        private static void AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
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