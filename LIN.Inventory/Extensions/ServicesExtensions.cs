using LIN.Inventory.Services.Reportes;

namespace LIN.Inventory.Extensions;

public static class ServicesExtensions
{

    /// <summary>
    /// Agregar LIN Services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services)
    {
        // Data.
        services.AddScoped<OutflowsReport, OutflowsReport>();
        services.AddScoped<ThirdPartyService, ThirdPartyService>();
        services.AddScoped<EmailSender, EmailSender>();

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