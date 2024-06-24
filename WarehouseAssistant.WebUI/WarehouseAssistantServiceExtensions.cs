using Microsoft.Extensions.DependencyInjection;
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

            return services;
        }
    }
}
