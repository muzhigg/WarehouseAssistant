using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

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
            
            return services;
        }

        private static void AddLocalStorage(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage(cfg =>
            {
                cfg.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                //cfg.JsonSerializerOptions.Converters.Add(
                //    new TypeMappingConverter<IFilterDefinition<Product>, FilterDefinition<Product>>());
                //cfg.JsonSerializerOptions.Converters.Add(
                //    new TypeMappingConverter<IFilterDefinition<MarketingMaterial>, FilterDefinition<MarketingMaterial>>());
                //cfg.JsonSerializerOptions.Converters.Add(
                //    new TypeMappingConverter<IFilterDefinition<ProductTableItem>, FilterDefinition<ProductTableItem>>());
                //cfg.JsonSerializerOptions.Converters.Add(new ObjectConverter());
            });
        }
    }
}
