using System.Reflection;

namespace LIN.Inventory.Controllers;


[Route("/")]
public class APIVersion : ControllerBase
{

    /// <summary>
    /// Obtiene la version de LIN Server Actual
    /// </summary>
    [HttpGet("Version")]
    public dynamic Version()
    {

        // Obtener el ensamblado actual (el ensamblado de la aplicación)
        Assembly? assembly = Assembly.GetEntryAssembly();

        // Obtener los metadatos del ensamblado
        AssemblyName? assemblyName = assembly?.GetName();
        string name = assemblyName?.Name ?? string.Empty;
        string version = assemblyName?.Version?.ToString() ?? string.Empty;
        string mode = "undefined";

#if DEBUG
        mode = "debug";
#elif RELEASE
        mode = "release";
#elif AZURE
        mode = "Azure";
#elif SOMEE
        mode = "Somee";
#endif
        // Retorna el resultado
        return new
        {
            Mode = mode,
            Name = name,
            Version = version,
            Open = $" {ServerLogger.OpenDate:HH:mm dd/MM/yyyy}"
        };

    }


    /// <summary>
    /// Obtiene el estado del servidor
    /// </summary>
    [HttpGet("status")]
    public dynamic Status()
    {
        return StatusCode(200, new
        {
            Status = "Open"
        });
    }


    /// <summary>
    /// Obtiene la lista de errores generados
    /// </summary>
    [HttpGet("logErros")]
    public dynamic Logs()
    {
        // Retorna el resultado
        return new
        {
            ServerLogger.Errors
        };

    }



    /// <summary>
    /// Obtiene la lista de conexiones
    /// </summary>
    [HttpGet("Conexiones")]
    public dynamic GetConexiones()
    {

        return new
        {
            ServerLogger.OpenConnections
        };

    }



    /// <summary>
    /// Obtiene la arquitectura de servidor
    /// </summary>
    [HttpGet("architecture")]
    public dynamic Architecture()
    {

        return new
        {
            ID = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            Arquitectura = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"),
            Counter = Environment.ProcessorCount
        };
    }


}
