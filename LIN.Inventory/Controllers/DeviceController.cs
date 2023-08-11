using LIN.Inventory.Hubs;
using LIN.Inventory.Services;

namespace LIN.Inventory.Controllers;


[Route("devices")]
public class DeviceController : ControllerBase
{


    /// <summary>
    /// Obtiene la lista de dispositivos asociados a una cuenta
    /// </summary>
    /// <param name="contextDevice">ID del dispositivo en SignalR</param>
    /// <param name="user">ID de la cuenta de usuario</param>
    [HttpGet]
    public HttpReadAllResponse<DeviceModel> GetAll([FromHeader] string contextDevice, [FromHeader] int user)
    {
        try
        {

            var devices = AccountHub.Cuentas.Where(T => T.Key == user)
                          .FirstOrDefault().Value
                          .Where(T => T.Estado == DeviceState.Actived).ToList();

            var device = devices.Where(T => T.ID == contextDevice).FirstOrDefault();

            if (device == null)
                return new(Responses.Undefined);

            device.Estado = DeviceState.Actived;
            // Retorna
            return new(Responses.Success, devices ?? new());
        }
        catch (Exception ex)
        {
            ServerLogger.LogError("No devices:" + ex.Message);
            return new(Responses.Undefined);
        }
    }



}
