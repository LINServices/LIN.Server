namespace LIN.Inventory.Controllers;


[Route("outflow")]
public class OutflowController : ControllerBase
{


    /// <summary>
    /// Nuevo movimiento de salida.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost("create")]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] OutflowDataModel modelo, [FromHeader] string token)
    {

        // Validar parámetros.
        if (!modelo.Details.Any() || modelo.Type == OutflowsTypes.None)
            return new(Responses.InvalidParam);


        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Acceso Iam.
        var iam = await Iam.OnInventory(modelo.InventoryId, tokenInfo.ProfileId);

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

        // Si no tiene el rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Establecer el modelo.
        modelo.Inventory = new()
        {
            ID = modelo.InventoryId
        };

        // Crea la nueva entrada
        var response = await Data.Outflows.Create(modelo);

        return response;

    }



    /// <summary>
    /// Obtiene una salida
    /// </summary>
    /// <param name="id">Id de la entrada</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<OutflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] string token, [FromHeader] bool includeDetails = false)
    {

        // Comprobaciones.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Obtener el inventario.
        var inventory = await Data.Inventories.FindByOutflow(id);

        // Si hubo un error.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el movimiento.",
                Response = Responses.Unauthorized
            };

        // Acceso Iam.
        var iam = await Iam.OnInventory(inventory.Model, tokenInfo.ProfileId);

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Outflows.Read(id, includeDetails);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene todas las salida asociadas a un inventario
    /// </summary>
    /// <param name="id">Id del inventario</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<OutflowDataModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.OnInventory(id, tokenInfo.ProfileId);

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no tienen permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Outflows.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }


}