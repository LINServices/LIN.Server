using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
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

        services.AddDbContextPool<Persistence.Context.Context>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Somee"));
        });
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
            context?.Database.EnsureCreated();
        }
        catch (Exception)
        {
        }
        return app;
    }

}