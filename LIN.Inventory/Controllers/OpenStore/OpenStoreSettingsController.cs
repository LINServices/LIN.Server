namespace LIN.Inventory.Controllers.OpenStore;

[InventoryToken]
[Route("[controller]")]
public class OpenStoreSettingsController(IOpenStoreSettingsRepository storeSettingsRepository, IIamService Iam) : ControllerBase
{

    [HttpPost]
    public async Task<CreateResponse> Create([FromQuery] int inventory, [FromQuery] string user, [FromQuery] string password, [FromHeader] string token, [FromHeader] string mercadoToken)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventory,
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

        // Autenticarse.
        var authResult = await Access.Developer.Controllers.Authentication.Login(user, password);

        if (authResult.Response != Responses.Success)
            return new();

        // Crear proyecto en LIN Cloud Orchestrator.
        var response = await LIN.Access.Developer.Controllers.Resources.Create(new()
        {
            Name = "Recurso de pagos en Inventario",
            Status = Types.Developer.Enumerations.ProjectStatus.Normal,
            Type = "payments"
        }, authResult.Token, new()
        {
            { "tokenName",mercadoToken }
        });

        // Obtener la llave.
        var key = await Access.Developer.Controllers.Keys.ReadAll(response.LastId, authResult.Token);

        if (key.Response != Responses.Success)
            return new()
            {
                Message = "No se pudo crear la llave.",
                Response = key.Response
            };

        return await storeSettingsRepository.Create(new()
        {
            ApiKey = key.Models.FirstOrDefault()?.Key ?? string.Empty,
            IsActive = true,
            IsActiveOnlinePayments = true,
            InventoryId = inventory
        });
    }


    [HttpGet]
    public async Task<ReadOneResponse<OpenStoreSettings>> Read([FromQuery] int inventory, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventory,
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

        // Obtener la información.
        var infor = await storeSettingsRepository.Read(inventory);

        return infor;
    }


    [HttpGet("payments")]
    public async Task<ReadAllResponse<PayModel>> Payments([FromQuery] int inventory, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventory,
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

        // Obtener la información.
        var infor = await storeSettingsRepository.Read(inventory);

        var payments = await LIN.Access.Payments.Controllers.Payments.ReadAll(infor.Model.ApiKey);


        return payments;
    }

}