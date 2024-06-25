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
            services.AddScoped<ProductRepository>();
            services.AddScoped<MarketingMaterialRepository>();
            services.AddMudServices();
            AddLocalStorage(services);

            return services;
        }

        private static void AddLocalStorage(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage(cfg =>
            {
                cfg.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                cfg.JsonSerializerOptions.Converters.Add(
                    new TypeMappingConverter<IFilterDefinition<Product>, FilterDefinition<Product>>());
                cfg.JsonSerializerOptions.Converters.Add(
                    new TypeMappingConverter<IFilterDefinition<MarketingMaterial>, FilterDefinition<MarketingMaterial>>());
            });
        }
    }
}
