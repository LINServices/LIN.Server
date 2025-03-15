namespace LIN.Inventory.Integrations;

[Route("connectors/[controller]")]
public class OpenStoreController(HoldsGroupRepository holdsRepository, Outflows outflowsRepository, OrdersRepository ordersRepository, OpenStoreSettingsRepository openStoreSettingsRepository, Products productsData, Outflows outflows) : ControllerBase
{

    public class WebhookRequest
    {
        public int OrderId { get; set; }
        public string Reference { get; set; }
        public int Status { get; set; }
        public string StatusString { get; set; }
    }

    /// <summary>
    /// Webhook para estados de los pagos en línea relacionados con el inventario.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Webhook([FromBody] WebhookRequest resultado)
    {

        // Buscar la orden
        var associated = await ordersRepository.ReadAll(resultado.Reference);

        foreach (var e in associated.Models)
        {
            // Encontrar inventario.
            var inventoryId = await holdsRepository.GetInventory(e.HoldGroupId);
            var holdItems = await holdsRepository.GetItems(e.HoldGroupId);


            // Actualizar el estado de la orden.
            List<Task> tasks = [
                            ordersRepository.Update(e.Id, resultado.StatusString),
                            holdsRepository.Approve(e.HoldGroupId)
            ];

            // Crear el movimiento si no existe y el estado es Ok.
            if (string.IsNullOrEmpty(e.Status) && resultado.StatusString == "Paid")
            {
                var salida = new OutflowDataModel()
                {
                    Order = e,
                    Date = DateTime.Now,
                    Status = MovementStatus.Accepted,
                    Id = 0,
                    Type = OutflowsTypes.Purchase,
                    Inventory = new() { Id = inventoryId.Model },
                    Profile = null,
                    ProfileId = 0,
                    OrderId = e.Id,
                    Details = []
                };

                foreach (var item in holdItems.Models)
                {
                    salida.Details.Add(new()
                    {
                        Quantity = item.Quantity,
                        Movement = salida,
                        ProductDetail = item.DetailModel
                    });
                }

                var response = await outflowsRepository.Create(salida);
                continue;
            }

            // Actualizar la información del movimiento si ya existe y el nuevo estado esta pagado.
            if (!string.IsNullOrEmpty(e.Status) && resultado.StatusString == "Paid")
            {

            }

            // Actualizar la información del movimiento si ya existe y el nuevo estado esta revertido.
            if (!string.IsNullOrEmpty(e.Status) && resultado.StatusString == "Reverted")
            {
                await outflows.Reverse(e.Id);
            }

            await Task.WhenAll(tasks);

        }



        // Cambiar el estado de la reserva a (Finalizado), evitara que se reintegre al inventario.




        // Registar movimiento de la venta en el inventario. (Devolución, venta...)

        /* Cuando un pago se devuelve, el estado del movimiento se actualiza */
        /* Cuando un pago se "paga", el estado del movimiento se actualiza a completado */



        return Ok();
    }

    /// <summary>
    /// Reservar stock de productos en un inventario especifico.
    /// </summary>
    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromBody] HoldGroupModel model)
    {

        List<HoldModel> holds = new();

        foreach (var item in model.Holds)
        {
            // Obtener producto.
            var product = await productsData.Read(item.DetailId);

            // Reservar stock (true / false)
            var holdModel = new HoldModel()
            {
                Status = HoldStatus.None,
                Quantity = item.Quantity,
                DetailId = product.Model.DetailModel?.Id ?? 0
            };
            holds.Add(holdModel);
        }

        model.Expiration = DateTime.Now.AddMinutes(10);
        model.Holds = holds;

        /* La reserva tiene una fecha limite, desde de ello, los productos se reintegraran al inventario */
        var response = await holdsRepository.Create(model);

        return Ok(response);
    }


    /// <summary>
    /// Eliminar una reserva.
    /// </summary>
    [HttpDelete("reserve")]
    public async Task<IActionResult> DeleteReserve([FromQuery] int holdGroupId)
    {

        // Eliminar la Reserva del stock/* Reintegrar los productos al inventario */
        await holdsRepository.Return(holdGroupId);



        return Ok();
    }


    /// <summary>
    /// Generar la orden de pago y enlace de pago para una reserva.
    /// </summary>
    [HttpPost("order")]
    public async Task<IActionResult> Order(int holdGroup)
    {

        string webhook = "https://sw3dtgpc-7019.use2.devtunnels.ms/connectors/OpenStore";

        /* Con el Id de la reserva, se dan 10 minutos mas, para el usuario tener tiempo de pagar, despues, se vence la orden y el enlace de pago */
        var grupo = await holdsRepository.GetItems(holdGroup);

        // Generar enlace de pago con Payments.
        var result = await LIN.Access.Payments.Controllers.Payments.Generate(webhook, "giraldojhong4@gmail.com", "1021804732", grupo.Models.Select(t => new Access.Payments.Controllers.PaymentItemDataModel()
        {
            Id = 0,
            Name = "Example",
            Picture = "",
            Price = t.DetailModel.SalePrice
        }));

        // Asociar la external Id con la orden local.
        // Crear la orden local.
        var order = new OrderModel
        {
            Id = 0,
            ExternalId = result.Models[1] ?? string.Empty,
            Status = "",
            HoldGroupId = holdGroup,
        };

        // Crear la orden.
        await ordersRepository.Create(order);

        // Retornar el enlace de pago.
        return Ok(result.Models[0]);
    }


    /// <summary>
    /// Buscar productos de uno o varios inventarios conectados con OpenStore.
    /// </summary>
    [HttpGet("search")]
    public IActionResult Search()
    {

        // Buscar productos en todos lo inventarios conectados (O en uno especifico) segun parametro.

        // Buscar por texto, descripcion, categoria.

        // Retornar productos.

        return Ok();
    }

}