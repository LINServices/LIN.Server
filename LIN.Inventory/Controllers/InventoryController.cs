namespace LIN.Inventory.Controllers;

[Route("api/[Controller]")]
[RateLimit(requestLimit: 40, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class InventoryController(IHubService hubService, IInventoriesRepository inventoryRepository, IIamService Iam) : ControllerBase
{

    /// <summary>
    /// Crea un nuevo Inventario.
    /// </summary>
    /// <param name="modelo">Modelo del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] InventoryDataModel modelo, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Comprobaciones
        if (!modelo.UsersAccess.Any() || !modelo.Name.Any() || !modelo.Direction.Any())
            return new(Responses.InvalidParam);

        // Establecer el creador.
        modelo.CreatorId = tokenInfo.ProfileId;

        // Modelo
        foreach (var access in modelo.UsersAccess)
        {
            access.Date = DateTime.Now;
            if (modelo.CreatorId == access.ProfileId)
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
        var response = await inventoryRepository.Create(modelo);

        // Si no se creo el inventario
        if (response.Response != Responses.Success)
            return response;

        // Enviar notificación.
        await hubService.SendNotification(response.LastId);

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
        var result = await inventoryRepository.ReadAll(tokenInfo.ProfileId);

        return result;

    }


    /// <summary>
    /// Obtener un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet]
    [InventoryToken]
    public async Task<HttpReadOneResponse<InventoryDataModel>> Read([FromQuery] int id, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles admitidos.
        InventoryRoles[] roles = [InventoryRoles.Administrator, InventoryRoles.Member, InventoryRoles.Guest, InventoryRoles.Supervisor, InventoryRoles.Reader];

        // Validar IamService.
        if (!roles.Contains(iam))
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes autorización."
            };


        // Crea el inventario
        var response = await inventoryRepository.Read(id);

        // Si no se creo el inventario
        if (response.Response != Responses.Success)
            return response;

        // Retorna
        return response;

    }


    /// <summary>
    /// Actualizar la información de un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="name">Nuevo nombre.</param>
    /// <param name="description">Nueva descripción.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPatch]
    [InventoryToken]
    public async Task<HttpResponseBase> Update([FromQuery] int id, [FromQuery] string name, [FromQuery] string description, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Validar IamService.
        if (iam != InventoryRoles.Administrator)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes autorización."
            };

        // Actualizar el rol.
        var response = await inventoryRepository.Update(id, name, description);

        // Retorna
        return response;

    }

}