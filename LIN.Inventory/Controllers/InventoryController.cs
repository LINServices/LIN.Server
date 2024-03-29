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

        // Obtener el Iam.
        var iam = await Iam.OnInventory(id, tokenInfo.ProfileId);

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

        // Obtener el Iam.
        var iam = await Iam.OnAccess(accessID, tokenInfo.ProfileId);

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







    ///////////////////----------------- Métodos a reemplazar -------------------






    /// <summary>
    /// Estadísticas del home
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="days">Cantidad de días atrás</param>
    [HttpGet("home")]
    public async Task<HttpReadOneResponse<HomeDto>> HomeService([FromHeader] int id, [FromHeader] int days)
    {

        if (id <= 0 || days < 0)
            return new(Responses.InvalidParam);



        var (context, contextKey) = Conexión.GetOneConnection();

        var ventas30 = await Data.Outflows.VentasOf(id, 30, context);
        var ventas7 = await Data.Outflows.VentasOf(id, 7, context);

        var compras30 = await Data.Inflows.ComprasOf(id, 30, context);
        var compras7 = await Data.Inflows.ComprasOf(id, 7, context);

        context.CloseActions(contextKey);

        return new ReadOneResponse<HomeDto>()
        {
            Response = Responses.Success,
            Model = new()
            {
                Compras30 = compras30.Model,
                Compras7 = compras7.Model,
                Ventas30 = ventas30.Model,
                Ventas7 = ventas7.Model
            }
        };
    }



    /// <summary>
    /// Obtiene la valuación de los inventarios donde un usuario es administrador
    /// </summary>
    /// <param name="id">Id del usuario</param>
    [HttpGet("valuation")]
    public async Task<HttpReadOneResponse<decimal>> Valuation([FromHeader] int id)
    {
        if (id <= 0)
            return new(Responses.InvalidParam);

        return await Data.Inventories.ValueOf(id);
    }






    [HttpGet("sales")]
    public async Task<HttpReadAllResponse<SalesModel>> Sales([FromHeader] int id, [FromHeader] int days)
    {

        if (id <= 0 || days < 0)
            return new(Responses.InvalidParam);

        var response = await Data.Outflows.Ventas(id, days);

        return response;
    }



}