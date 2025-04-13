namespace LIN.Inventory.Controllers;

[InventoryToken]
[Route("[Controller]")]
[RateLimit(requestLimit: 40, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class HoldsController(IHoldsGroupRepository holdsGroupRepository, IIamService Iam) : ControllerBase
{

    /// <summary>
    /// Obtener los productos que actualmente esta reservados.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<HoldModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await holdsGroupRepository.GetItemsHolds(id);
        return result;

    }


    /// <summary>
    /// Realizar la devolución de una reserva.
    /// </summary>
    /// <param name="id">Id del grupo del hold.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpDelete]
    public async Task<HttpResponseBase> Return([FromHeader] int id, [FromHeader] string token)
    {

        // Parámetros.
        if (id < 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener el inventario.
        var inventory = await holdsGroupRepository.GetInventory(id);

        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el inventario asociado.",
                Response = Responses.Unauthorized
            };

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventory.Model,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Realizar la devolución de la reserva.
        ResponseBase response = await holdsGroupRepository.Return(id);

        return response;
    }

}