using LIN.Inventory.Data;

namespace LIN.Inventory.Extensions;

public static class ServicesExtensions
{

    /// <summary>
    /// Agregar LIN Services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services)
    {

        // Data.
        services.AddScoped<Inflows, Inflows>();
        services.AddScoped<Inventories, Inventories>();
        services.AddScoped<InventoryAccess, InventoryAccess>();
        services.AddScoped<Outflows, Outflows>();
        services.AddScoped<Products, Products>();
        services.AddScoped<Profiles, Profiles>();
        services.AddScoped<Statistics, Statistics>();

        // Hubs.
        services.AddScoped<IHubService, HubService>();

        // Iam.
        services.AddScoped<IIam, Iam>();

        return services;

    }


    /// <summary>
    /// Agregar LIN Services.
    /// </summary>
    public static IApplicationBuilder UseLocalServices(this IApplicationBuilder app, IConfigurationManager configuration)
    {
        Jwt.Set(configuration["jwt:key"] ?? string.Empty);
        return app;
    }

}