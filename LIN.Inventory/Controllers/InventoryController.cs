namespace LIN.Inventory.Controllers;


[Route("Inventory")]
public class InventoryController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo Inventario.
    /// </summary>
    /// <param name="modelo">Modelo del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost("create")]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] InventoryDataModel modelo, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Comprobaciones
        if (!modelo.UsersAccess.Any() || !modelo.Nombre.Any() || !modelo.Direction.Any())
            return new(Responses.InvalidParam);

        // Establecer el creador.
        modelo.Creador = tokenInfo.ProfileId;

        // Modelo
        foreach (var access in modelo.UsersAccess)
        {
            access.Fecha = DateTime.Now;
            if (modelo.Creador == access.ProfileID)
            {
                access.Rol = InventoryRoles.Administrator;
                access.State = InventoryAccessState.Accepted;
            }
            else
            {
                access.State = InventoryAccessState.OnWait;
            }
        }

        // Crea el inventario
        var response = await Data.Inventories.Create(modelo);

        // Si no se creo el inventario
        if (response.Response != Responses.Success)
            return response;

        // Retorna
        return response;

    }



    /// <summary>
    /// Obtiene los inventarios asociados a un perfil
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<InventoryDataModel>> ReadAll([FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtiene la lista de Id's de inventarios
        var result = await Data.Inventories.ReadAll(tokenInfo.ProfileId);

        return result;

    }




    /// <summary>
    /// Obtener un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<InventoryDataModel>> Read([FromQuery] int id, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles admitidos.
        InventoryRoles[] roles = [InventoryRoles.Administrator, InventoryRoles.Member, InventoryRoles.Guest];

        // Validar Iam.
        if (!roles.Contains(iam))
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes autorización."
            };


        // Crea el inventario
        var response = await Data.Inventories.Read(id);

        // Si no se creo el inventario
        if (response.Response != Responses.Success)
            return response;

        // Retorna
        return response;

    }




    /// <summary>
    /// Actualiza el rol de un usuario en un inventario
    /// </summary>
    /// <param name="accessID">Id del acceso</param>
    /// <param name="newRol">Nuevo rol</param>
    /// <param name="token">Token de acceso</param>
    [HttpPatch("update/rol")]
    [InventoryToken]
    public async Task<HttpResponseBase> UpdateRol([FromHeader] int accessID, [FromHeader] InventoryRoles newRol, [FromHeader] string token)
    {

        // Comprobaciones
        if (accessID <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Access,
            Id = accessID,
            Profile = tokenInfo.ProfileId
        });

        // Validar Iam.
        if (iam != InventoryRoles.Administrator)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes autorización."
            };


        // Actualizar el rol.
        var response = await Data.InventoryAccess.UpdateRol(accessID, newRol);

        // Retorna
        return response;

    }


}