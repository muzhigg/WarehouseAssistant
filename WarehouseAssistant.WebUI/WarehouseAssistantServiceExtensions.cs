using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI
{
    public static class WarehouseAssistantServiceExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddWebUIServices(this IServiceCollection services)
        {
            // services.AddScoped<MarketingMaterialRepository>();
            services.AddMudServices(config =>
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
            
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<ReceivingItemRepository>();
            
            return services;
        }
        
        private static void AddLocalStorage(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage(cfg => { cfg.JsonSerializerOptions.IgnoreReadOnlyProperties = true; });
        }
    }
}