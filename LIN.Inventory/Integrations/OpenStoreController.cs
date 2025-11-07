namespace LIN.Inventory.Integrations;

[Route("connectors/[controller]")]
public class OpenStoreController(IHubService hubService, IHoldsGroupRepository holdsRepository, IThirdPartyService thirdPartyService, IOutflowsRepository outflowsRepository, IOrdersRepository ordersRepository, IProductsRepository productsData, IOutflowsRepository outflows, IIamService Iam, IEmailSenderService emailSender, IInflowsRepository inflowsRepository, IOpenStoreSettingsRepository openStoreSettingsRepository) : ControllerBase
{

    /// <summary>
    /// Webhook para estados de los pagos en línea relacionados con el inventario.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Webhook([FromBody] WebhookRequest resultado)
    {
        // Buscar la orden
        var associated = await ordersRepository.ReadAll(resultado.Reference);

        foreach (var order in associated.Models)
        {
            // Encontrar inventario.
            var inventoryId = await holdsRepository.GetInventory(order.HoldGroupId);
            var holdItems = await holdsRepository.GetItems(order.HoldGroupId);

            await ordersRepository.Update(order.Id, resultado.StatusString);

            // Si esta pendiente pago, no hacemos nada.
            if (resultado.StatusString == "PaymentRequired" || resultado.StatusString == "Pending")
            {
                continue;
            }

            // Crear la información del movimiento si ya existe y el nuevo estado esta pagado.
            if (resultado.StatusString == "Paid")
            {
                // Aprobar la reserva.
                await holdsRepository.Approve(order.HoldGroupId);

                var has = await ordersRepository.HasMovements(order.Id);

                // Validar si hay movimientos en la orden, en caso de no, lo crea.
                if (!has.Model && has.Response == Responses.Success)
                {
                    // Crear el movimiento.
                    await CreateMovement(order, holdItems.Models, resultado.Payer, inventoryId.Model);
                }
            }

            // Si el pago fue rechazado.
            if (!string.IsNullOrEmpty(order.Status) && (resultado.StatusString == "Rejected"))
            {
                await holdsRepository.Return(order.HoldGroupId);
            }

            // Si el tiempo expiro.
            if (resultado.StatusString == "Expired")
            {
                await holdsRepository.Return(order.HoldGroupId);
            }

            // Actualizar la información del movimiento si ya existe y el nuevo estado esta revertido.
            if (!string.IsNullOrEmpty(order.Status) && resultado.StatusString == "Reverted")
            {
                var response = await outflows.Reverse(order.Id);

                // Notificar en tiempo real.
                if (response.Response == Responses.Success)
                {
                    var inventory = await inflowsRepository.GetInventory(response.LastId);
                    await hubService.SendInflowMovement(inventory.Model, response.LastId);
                }

            }

        }

        return Ok();
    }


    /// <summary>
    /// Reservar stock de productos en un inventario especifico.
    /// </summary>
    [HttpPost("hold")]
    [InventoryToken]
    public async Task<HttpCreateResponse> Reserve([FromBody] OutflowDataModel modelo, [FromHeader] string token)
    {

        // Validar parámetros.
        if (modelo.Details.Count == 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = modelo.InventoryId,
            Profile = tokenInfo.ProfileId
        });

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene el rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Validar tercero.
        if (modelo.Outsider is null || string.IsNullOrWhiteSpace(modelo.Outsider.Document) || string.IsNullOrWhiteSpace(modelo.Outsider.Email))
            return new()
            {
                Message = "Para realizar una venta en línea se debe incluir un cliente final.",
                Response = Responses.InvalidParam
            };

        // Obtener o crear el cliente.
        var client = await thirdPartyService.FindOrCreate(modelo.Outsider, modelo.InventoryId);

        // Si todo es ok se establece el modelo.
        if (client.Response == Responses.Success)
        {
            modelo.Outsider = client.Model;
        }
        // Retornamos el error.
        else
        {
            return new(Responses.InvalidParam)
            {
                Message = $"No se pudo crear u obtener un cliente con el documento '{modelo.Outsider.Document}'"
            };
        }

        // Crear la reserva.
        var items = modelo.Details.Select(t => new { Id = t.ProductDetailId, t.Quantity });

        // Holds.
        List<HoldModel> holds = [];

        foreach (var item in items)
        {
            // Reservar stock (true / false)
            var holdModel = new HoldModel()
            {
                Status = HoldStatus.None,
                Quantity = item.Quantity,
                DetailId = item.Id
            };
            holds.Add(holdModel);
        }

        // Modelo.
        HoldGroupModel group = new()
        {
            Id = 0,
            Expiration = DateTime.Now.AddMinutes(10),
            Holds = holds,
        };

        /* La reserva tiene una fecha limite, desde de ello, los productos se reintegraran al inventario */
        var response = await holdsRepository.Create(group);

#if DEBUG
        string webhook = "https://sw3dtgpc-7019.use2.devtunnels.ms/connectors/OpenStore";
#else
        string webhook = "https://api.linplatform.com/inventory/connectors/OpenStore";
#endif

        /* Con el Id de la reserva, se dan 10 minutos mas, para el usuario tener tiempo de pagar, despues, se vence la orden y el enlace de pago */
        var grupo = await holdsRepository.GetItems(response.LastId);

        // Obtener información del key.
        var settings = await openStoreSettingsRepository.Read(modelo.InventoryId);

        // Generar enlace de pago con Payments.
        var result = await Access.Payments.Controllers.Preferences.Create(webhook, client.Model.Email, client.Model.Document, settings.Model.ApiKey, DateTime.Now.AddMinutes(2), grupo.Models.Select(t => new PaymentItemRequestModel()
        {
            Id = 0,
            Name = t.DetailModel.Product.Name,
            Picture = "",
            Quantity = t.Quantity,
            Price = t.DetailModel.SalePrice
        }));

        // Si no se crea el enlace.
        if (result.Response != Responses.Success)
        {
            // Eliminar la reserva.
            await holdsRepository.Return(response.LastId);
            return new(Responses.InvalidParam)
            {
                Message = "No se pudo generar el enlace de pago, intente nuevamente."
            };
        }

        // Asociar la external Id con la orden local.
        // Crear la orden local.
        var order = new OrderModel
        {
            Id = 0,
            ExternalId = result.Models[1] ?? string.Empty,
            Status = "",
            HoldGroupId = response.LastId
        };

        // Crear la orden.
        var orderRepo = await ordersRepository.Create(order);

        // Enviar correo al cliente.
        await emailSender.Send(client.Model.Email, "Nueva orden de pago", System.IO.File.ReadAllText("wwwroot/Plantillas/Payment.html"));

        // ------------------------------------
        return new CreateResponse(Responses.Success, orderRepo.LastId)
        {
            Alternatives = [result.Models[0]],
            LastUnique = result.Models[0]
        };
    }


    /// <summary>
    /// Reservar stock de productos en un inventario especifico.
    /// </summary>
    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromBody] HoldGroupModel model)
    {

        List<HoldModel> holds = [];

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




    private async Task CreateMovement(OrderModel order, IEnumerable<HoldModel> holds, PayerRequest? payer, int inventory)
    {
        // Modelo de salida.
        var outflow = new OutflowDataModel()
        {
            Order = order,
            Date = DateTime.Now,
            Status = MovementStatus.Approved,
            Id = 0,
            Type = OutflowsTypes.Purchase,
            Inventory = new() { Id = inventory },
            Profile = null,
            ProfileId = 0,
            OrderId = order.Id,
            Details = []
        };

        // Obtener el cliente.
        if (!string.IsNullOrWhiteSpace(payer?.Document))
        {
            // Obtener o crear el tercero.
            var third = await thirdPartyService.FindOrCreate(new()
            {
                Name = payer.Name,
                Type = OutsiderTypes.Person,
                Document = payer.Document,
                Email = payer.Mail
            }, inventory);

            // Establecer el tercero.
            outflow.Outsider = (third.Response == Responses.Success) ? third.Model : null;
        }

        foreach (var item in holds)
        {
            outflow.Details.Add(new()
            {
                Quantity = item.Quantity,
                Movement = outflow,
                ProductDetail = item.DetailModel
            });
        }

        // Creamos el movimiento, sin actualizar el inventario (Porque la reserva ya los tiene).
        var response = await outflowsRepository.Create(outflow, updateInventory: false);

        // Notificar en tiempo real.
        if (response.Response == Responses.Success)
            await hubService.SendOutflowMovement(inventory, response.LastId);

    }

}