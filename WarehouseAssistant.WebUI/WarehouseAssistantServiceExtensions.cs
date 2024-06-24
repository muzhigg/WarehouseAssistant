using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
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

            return services;
        }
    }
}
