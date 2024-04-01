namespace LIN.Inventory.Controllers;


[Route("inflow")]
public class InflowController : ControllerBase
{


    /// <summary>
    /// Crear nueva movimiento de entrada.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost("create")]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] InflowDataModel modelo, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Establecer el perfil.
        modelo.ProfileID = tokenInfo.ProfileId;

        // Comprobaciones
        if (!modelo.Details.Any() || modelo.Type == InflowsTypes.Undefined)
            return new(Responses.InvalidParam);

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = modelo.InventoryId,
            Profile = tokenInfo.ProfileId
        });

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Generar el modelo.
        modelo.Inventory = new()
        {
            ID = modelo.InventoryId
        };

        // Crea la nueva entrada.
        var response = await Data.Inflows.Create(modelo);

        // Respuesta.
        return response;

    }



    /// <summary>
    /// Obtener el movimiento (entrada).
    /// </summary>
    /// <param name="id">Id de la entrada.</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<InflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] string token, [FromHeader] bool includeDetails = false)
    {

        // Validar parámetros.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inflow,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Inflows.Read(id, includeDetails);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene las entradas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<InflowDataModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Validar parámetros.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Inflows.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Actualizar la fecha de una entrada.
    /// </summary>
    /// <param name="id">Id de la entrada.</param>
    /// <param name="date">Nueva fecha.</param>
    /// <param name="token">Token de acceso</param>
    [HttpPatch]
    [InventoryToken]
    public async Task<HttpResponseBase> Update([FromHeader] int id, [FromQuery] DateTime date, [FromHeader] string token)
    {

        // Validar parámetros.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inflow,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Inflows.Update(id, date);

        // Retorna el resultado
        return result ?? new();

    }



}