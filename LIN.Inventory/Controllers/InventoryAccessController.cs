namespace LIN.Inventory.Controllers;


[Route("inventory/access")]
public class InventoryAccessController(IHubContext<InventoryHub> hubContext) : ControllerBase
{


    /// <summary>
    /// Hub de contexto.
    /// </summary>
    private readonly IHubContext<InventoryHub> _hubContext = hubContext;




    /// <summary>
    /// Crear acceso a inventario.
    /// </summary>
    /// <param name="model">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] InventoryAcessDataModel model, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = model.Inventario,
            Profile = tokenInfo.ProfileId
        });

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Data.
        model.State = InventoryAccessState.OnWait;
        model.Fecha = DateTime.Now;

        // Crear acceso.
        var result = await Data.InventoryAccess.Create(model);

        // Si el recurso ya existe.
        if (result.Response == Responses.ResourceExist)
        {
            var update = await Data.InventoryAccess.UpdateState(result.LastID, InventoryAccessState.OnWait);
            result.Response = update.Response;
        }

        // Si fue correcto.
        if (result.Response == Responses.Success)
        {
            // Realtime.
            string groupName = $"group.{model.ProfileID}";
            string command = $"newInvitation({result.LastID})";
            await _hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
            {
                Command = command
            });
        }

        // Retorna el resultado
        return new CreateResponse()
        {
            Response = result.Response,
            LastID = result.LastID
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
        var result = await Data.InventoryAccess.Read(id);

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
        var result = await Data.InventoryAccess.ReadAll(tokenInfo.ProfileId);

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
        var result = await Data.InventoryAccess.UpdateState(id, estado);

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
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Actualizar el rol.
        var result = await Data.InventoryAccess.UpdateRol(id, rol);

        // Retorna el resultado
        return result;

    }




    /// <summary>
    /// Obtiene la lista de integrantes asociados a un inventario
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
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };


        // Obtiene la lista de Id's de inventarios
        var result = await Data.InventoryAccess.ReadMembers(inventario);


        var map = result.Models.Select(T => T.Item2.AccountID).ToList();

        var users = await LIN.Access.Auth.Controllers.Account.Read(map, tokenAuth);


        var i = (from I in result.Models
                 join A in users.Models
                 on I.Item2.AccountID equals A.Id
                 select new IntegrantDataModel
                 {
                     State = I.Item1.State,
                     AccessID = I.Item1.ID,
                     InventoryID = I.Item1.Inventario,
                     Nombre = A.Name,
                     Perfil = A.Profile,
                     ProfileID = I.Item2.ID,
                     Rol = I.Item1.Rol,
                     Usuario = A.Identity.Unique
                 }).ToList();



        return new(Responses.Success, i);

    }



    /// <summary>
    /// Elimina a alguien de un inventario
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
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene la lista de Id's de inventarios
        var result = await Data.InventoryAccess.DeleteSomeOne(inventario, usuario);

        return result;

    }


}