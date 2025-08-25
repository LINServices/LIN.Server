using LIN.Types.Payments.Enums;

namespace LIN.Inventory.Controllers;

[InventoryToken]
[Route("[Controller]")]
[RateLimit(requestLimit: 40, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class HoldsController(IHoldsGroupRepository holdsGroupRepository, IOrdersRepository ordersRepository, IIamService Iam) : ControllerBase
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

        // Validar que no tenga una transacción pendiente.
        var hold = await holdsGroupRepository.Read(id);

        if (hold.Response != Responses.Success)
            return new()
            {
                Message = "No se encontró el hold.",
                Response = Responses.NotRows,
                Errors = [new() { Description = "No hay una reserva asociada." }]
            };

        if (hold.Model.Expiration > DateTime.Now)
            return new()
            {
                Message = "El hold no ha expirado.",
                Response = Responses.Unauthorized,
                Errors = [new() { Description = "La reserva aun no ha expirado." }]
            };

        // Validar el estado de la orden.
        var order = await ordersRepository.ReadByHold(id);

        if (order.Response != Responses.Success)
            return new()
            {
                Message = "No se encontró la orden asociada al hold.",
                Response = Responses.NotRows,
                Errors = [new() { Description = "No hay una orden asociada a la reserva, por favor comuníquese con soporte." }]
            };

        // Buscar estado real en LIN Payments.
        var orders = await Access.Payments.Controllers.Payments.ReadOrders(order.Model.ExternalId);

       var status = orders.Models.Select(t => t.Status);

        if (status.Contains(OrderStatusEnum.PartiallyPaid))
            return new()
            {
                Message = "La orden aun esta pendiente de pago parcial en Mercado Pago.",
                Response = Responses.Unauthorized,
                Errors = [new() { Description = "El cliente realizo un pago parcial sobre la orden, debe esperar a que cancele todo completo o se reverse el pago para finalizar el proceso." }]
            };

        if (status.Contains(OrderStatusEnum.PaymentRequired) || status.Contains(OrderStatusEnum.Pending))
            return new()
            {
                Message = "La orden aun esta pendiente de pago en Mercado Pago.",
                Response = Responses.Unauthorized,
                Errors = [new() { Description = "El pago del cliente aun no se ha terminado o autorizado, debe esperar a finalizar el proceso." }]
            };

        // Realizar la devolución de la reserva.
        ResponseBase response = await holdsGroupRepository.Return(id);

        return response;
    }

}