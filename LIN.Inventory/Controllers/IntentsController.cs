using LIN.Inventory.Hubs;
using LIN.Inventory.Services;

namespace LIN.Inventory.Controllers;


[Route("Intents")]
public class IntentsController : ControllerBase
{

    /// <summary>
    /// Obtiene la lista de intentos Passkey activos
    /// </summary>
    /// <param name="contextDevice">Dispositivo de contexto</param>
    /// <param name="user">Usuario</param>
    [HttpGet]
    public HttpReadAllResponse<PasskeyIntentDataModel> GetAll([FromHeader] string contextDevice, [FromHeader] string user)
    {
        try
        {


            // Cuenta
            var account = (from A in PassKeyHub.PassKeyIntents
                           where A.Key.ToLower() == user.ToLower()
                           select A).FirstOrDefault().Value ?? new();

            // Hora actual
            var timeNow = DateTime.Now;

            // Intentos
            var intentos = (from I in account
                            where I.Status == PassKeyStatus.Undefined
                            where I.Expiracion > timeNow
                            select I).ToList();

            // Retorna
            return new(Responses.Success, intentos);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError($"No devices: ({contextDevice})" + ex.Message);
            return new(Responses.Undefined);
        }
    }



}
