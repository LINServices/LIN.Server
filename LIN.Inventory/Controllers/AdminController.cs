using LIN.Inventory.Services;

namespace LIN.Inventory.Controllers;


[Route("reload")]
public class AdminController : ControllerBase
{


    /// <summary>
    /// Establece una llave para LIN Developers
    /// </summary>
    /// <param name="key">Nueva llave</param>
    /// <param name="user">Usuario</param>
    /// <param name="pass">Contraseña</param>
    [HttpPost("set/Key")]
    public async Task<ActionResult<int>> Logs([FromHeader] string key, [FromHeader] string user, [FromHeader] string pass)
    {

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Obtener la dirección IP del cliente
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        stopwatch.Stop();
        ServerLogger.LogError("IP: " + ipAddress);

        ServerLogger.LogError("1: " + stopwatch.Elapsed.TotalMilliseconds);

        var admin = await Data.Profiles.Read(user);

        var old = EncryptClass.Encrypt(Conexión.SecreteWord + pass);

        if (admin.Response != Responses.Success || admin.Model.Contraseña != old)
            return StatusCode(401, "Usuario Invalido o contraseña incorrecta");

        if (admin.Model.Rol != UserRol.Admin)
            return StatusCode(401, "Debes ser un usuario ADMIN");




        Developers.SetKey(key);
        return Ok("ok");

    }



    /// <summary>
    /// Obtiene la IP del cliente
    /// </summary>
    [HttpGet("my/ip")]
    public IActionResult ObtenerDirecciónIP([FromServices] IHttpContextAccessor httpContextAccessor)
    {

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Obtener el contexto HTTP actual
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
            return NoContent();

        // Obtener la dirección IP del cliente
        var ipAddress = httpContext.Connection.RemoteIpAddress;

        // Verificar si la dirección IP es de IPv4 o IPv6
        if (ipAddress != null)
        {
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }

            // ipAddress ahora contiene la dirección IP del cliente
            var ipString = ipAddress.ToString();
            ServerLogger.LogError("2: " + stopwatch.Elapsed.TotalMilliseconds);
            return Ok(ipString);
        }

        // No se pudo obtener la dirección IP del cliente
        return NotFound();
    }


}