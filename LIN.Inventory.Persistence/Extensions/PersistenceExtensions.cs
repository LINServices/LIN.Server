using LIN.Inventory.Persistence.Repositories;
using LIN.Inventory.Persistence.Repositories.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LIN.Inventory.Persistence.Extensions;

public static class PersistenceExtensions
{

    /// <summary>
    /// Agregar persistencia.
    /// </summary>
    /// <param name="services">Servicios.</param>
    /// <param name="configuration">Configuración.</param>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfigurationManager configuration)
    {

        services.AddDbContextPool<Context.Context>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Somee"));
        });

        // Servicios de datos.
        services.AddScoped<IInflowsRepository, InflowsRepository>();
        services.AddScoped<IOutflowsRepository, OutflowsRepository>();
        services.AddScoped<IInventoriesRepository, InventoryRepository>();
        services.AddScoped<IInventoryAccessRepository, InventoryAccessRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IProfilesRepository, ProfilesRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<IHoldsRepository, HoldsRepository>();
        services.AddScoped<IOpenStoreSettingsRepository, OpenStoreSettingsRepository>();
        services.AddScoped<IHoldsGroupRepository, HoldsGroupRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IOutsiderRepository, OutsiderRepository>();

        return services;

    }


    /// <summary>
    /// Utilizar persistencia.  
    /// </summary>
    public static IApplicationBuilder UsePersistence(this IApplicationBuilder app)
    {
        try
        {
            var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetService<Persistence.Context.Context>();
            bool? created = context?.Database.EnsureCreated();
        }
        catch (Exception)
        {
        }
        return app;
    }

}