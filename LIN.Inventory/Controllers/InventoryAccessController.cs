namespace LIN.Inventory.Controllers;

[Route("Inventory/access")]
[RateLimit(requestLimit: 40, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class InventoryAccessController(IHubService hubService, IInventoryAccessRepository inventoryAccessRepository, IIam Iam) : ControllerBase
{

    /// <summary>
    /// Crear acceso a inventario.
    /// </summary>
    /// <param name="model">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] InventoryAccessDataModel model, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = model.InventoryId,
            Profile = tokenInfo.ProfileId
        });

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Data.
        model.State = InventoryAccessState.OnWait;
        model.Date = DateTime.Now;

        // Crear acceso.
        var result = await inventoryAccessRepository.Create(model);

        // Si el recurso ya existe.
        if (result.Response == Responses.ResourceExist)
        {
            var update = await inventoryAccessRepository.UpdateState(result.LastId, InventoryAccessState.OnWait);
            result.Response = update.Response;
        }

        // Si fue correcto.
        if (result.Response == Responses.Success)
        {
            // Enviar en tiempo real.
            await hubService.SendNotification(model.ProfileId, result.LastId);
        }

        // Retorna el resultado
        return new CreateResponse()
        {
            Response = result.Response,
            LastId = result.LastId
        };

    }


    /// <summary>
    /// Obtener una notificación.
    /// </summary>
    /// <param name="id">Id de la notificación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet]
    [InventoryToken]
    public async Task<HttpReadOneResponse<Notificacion>> Read([FromHeader] int id, [FromHeader] string token)
    {

        // Información del token.
        _ = HttpContext.Items[token] as JwtInformation ?? new();



        // Obtiene la lista de Id's de inventarios
        var result = await inventoryAccessRepository.Read(id);

        // Retorna el resultado
        return result;

    }


    /// <summary>
    /// Obtiene una lista de accesos asociados a un usuario.
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<Notificacion>> ReadAll([FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtiene la lista de Id's de inventarios
        var result = await inventoryAccessRepository.ReadAll(tokenInfo.ProfileId);

        // Retorna el resultado
        return result;

    }


    /// <summary>
    /// Cambia el acceso al inventario por medio de su Id
    /// </summary>
    /// <param name="id">Id del estado de inventario</param>
    /// <param name="estado">Nuevo estado del acceso</param>
    [HttpPut("update/state")]
    [InventoryToken]
    public async Task<HttpResponseBase> AccessChange([FromHeader] string token, [FromHeader] int id, [FromHeader] InventoryAccessState estado)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Comprobaciones
        if (id <= 0 || estado == InventoryAccessState.Undefined)
            return new(Responses.InvalidParam);


        var can = await Iam.CanAccept(id, tokenInfo.ProfileId);

        if (!can)
            return new(Responses.Unauthorized);


        // Obtiene la lista de Id's de inventarios
        var result = await inventoryAccessRepository.UpdateState(id, estado);

        // Retorna el resultado
        return result;

    }


    /// <summary>
    /// Actualizar el rol.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="id">Id del acceso.</param>
    /// <param name="rol">Nuevo rol.</param>
    [HttpPut("update/rol")]
    [InventoryToken]
    public async Task<HttpResponseBase> UpdateRol([FromHeader] string token, [FromQuery] int id, [FromQuery] InventoryRoles rol)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Access,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Si se trata de escalar roles.
        if (iam != InventoryRoles.Administrator && rol == InventoryRoles.Administrator)
            return new()
            {
                Message = "No tienes suficientes privilegios para actualizar el rol a administrador.",
                Response = Responses.Unauthorized
            };

        // Actualizar el rol.
        var result = await inventoryAccessRepository.UpdateRol(id, rol);

        // Retorna el resultado
        return result;

    }


    /// <summary>
    /// Obtiene la lista de integrantes asociados a un inventario.
    /// </summary>
    /// <param name="inventario">Id del inventario</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("members")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<IntegrantDataModel>> ReadAll([FromHeader] int inventario, [FromHeader] string token, [FromHeader] string tokenAuth)
    {

        // Comprobaciones
        if (inventario <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventario,
            Profile = tokenInfo.ProfileId
        });

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Supervisor, InventoryRoles.Administrator, InventoryRoles.Reader];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };


        // Obtiene la lista de Id's de inventarios
        var result = await inventoryAccessRepository.ReadMembers(inventario);


        var map = result.Models.Select(T => T.Item2.AccountId).ToList();

        var users = await LIN.Access.Auth.Controllers.Account.Read(map, tokenAuth);


        var i = (from I in result.Models
                 join A in users.Models
                 on I.Item2.AccountId equals A.Id
                 select new IntegrantDataModel
                 {
                     State = I.Item1.State,
                     AccessId = I.Item1.Id,
                     InventoryId = I.Item1.InventoryId,
                     Name = A.Name,
                     ProfileId = I.Item2.Id,
                     Rol = I.Item1.Rol,
                     User = A.Identity.Unique
                 }).ToList();



        return new(Responses.Success, i);

    }


    /// <summary>
    /// Elimina a alguien de un inventario.
    /// </summary>
    /// <param name="inventario">Id del inventario</param>
    /// <param name="usuario">Id del usuario que va a ser eliminado</param>
    /// <param name="token">Token de acceso.</param>
    [HttpDelete("delete/one")]
    [InventoryToken]
    public async Task<HttpResponseBase> DeleteSomeOne([FromHeader] int inventario, [FromHeader] int usuario, [FromHeader] string token)
    {

        // Comprobaciones
        if (inventario <= 0 || usuario <= 0)
            return new(Responses.InvalidParam);


        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventario,
            Profile = tokenInfo.ProfileId
        });

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene la lista de Id's de inventarios
        var result = await inventoryAccessRepository.DeleteSomeOne(inventario, usuario);

        return result;

    }

}