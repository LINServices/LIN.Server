namespace LIN.Inventory.Controllers.OpenStore;

[InventoryToken]
[Route("[controller]")]
public class OpenStoreSettingsController(IOpenStoreSettingsRepository storeSettingsRepository, IIamService Iam) : ControllerBase
{

    [HttpPost]
    public async Task<CreateResponse> Create(OpenStoreSettings model, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = model.InventoryId,
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

        return await storeSettingsRepository.Create(model);
    }

}