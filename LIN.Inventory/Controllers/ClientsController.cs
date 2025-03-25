namespace LIN.Inventory.Controllers;

[InventoryToken]
[Route("[Controller]")]
[RateLimit(requestLimit: 40, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class ClientsController(IIamService Iam, IOutsiderRepository outsiderRepository) : ControllerBase
{

    /// <summary>
    /// Buscar.
    /// </summary>
    /// <param name="pattern">Patron de búsqueda.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("search")]
    public async Task<HttpReadAllResponse<OutsiderModel>> SearchOutsiders([FromQuery] string pattern, [FromHeader] int inventory, [FromHeader] string token)
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

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor, InventoryRoles.Member];

        // Si no tiene el rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        return await outsiderRepository.Search(pattern, inventory);

    }

}