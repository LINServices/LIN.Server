namespace LIN.Inventory.Controllers;


[Route("[controller]")]
public class DevicesController : ControllerBase
{

    /// <summary>
    /// Obtener dispositivos.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    [HttpGet]
    [InventoryToken]
    public HttpReadAllResponse<DeviceModel> Devices([FromHeader] string token)
    {
        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener los dispositivos.
        var devices = (from profile in InventoryHub.List
                       where profile.Key == tokenInfo.ProfileId
                       select profile.Value).FirstOrDefault();

        // Respuesta.
        return new ReadAllResponse<DeviceModel>()
        {
            Response = Responses.Success,
            Models = devices
        };
    }

}