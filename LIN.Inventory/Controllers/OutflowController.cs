using LIN.Inventory.Services.Reportes;

namespace LIN.Inventory.Controllers;

[Route("[Controller]")]
[RateLimit(requestLimit: 40, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class OutflowController(IHubService hubService, IOutflowsRepository outflowRepository, IThirdPartyService thirdPartyService, IIamService Iam, OutflowsReport outflowReport) : ControllerBase
{

    /// <summary>
    /// Nuevo movimiento de salida.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] OutflowDataModel modelo, [FromHeader] string token)
    {

        // Validar parámetros.
        if (modelo.Details.Count == 0 || modelo.Type == OutflowsTypes.None)
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
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor, InventoryRoles.Member];

        // Si no tiene el rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Validar tercero.
        if (modelo.Outsider is not null)
        {
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
        }
        else
        {
            modelo.Outsider = null;
            modelo.OutsiderId = null;
        }

        // Establecer el modelo.
        modelo.Inventory = new()
        {
            Id = modelo.InventoryId
        };
        modelo.ProfileId = tokenInfo.ProfileId;

        // Crea la nueva entrada
        var response = await outflowRepository.Create(modelo);

        // Enviar notificación en tiempo real.
        if (response.Response == Responses.Success)
            await hubService.SendOutflowMovement(modelo.InventoryId, response.LastId);

        return response;
    }


    /// <summary>
    /// Obtiene una salida
    /// </summary>
    /// <param name="id">Id de la entrada</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet]
    [InventoryToken]
    public async Task<HttpReadOneResponse<OutflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] string token, [FromHeader] bool includeDetails = false, [FromHeader] string identityToken = "")
    {

        // Comprobaciones.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Outflow,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor, InventoryRoles.Member, InventoryRoles.Reader];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await outflowRepository.Read(id, includeDetails);

        if (result.Model.Profile?.AccountId > 0)
        {
            var response = await Access.Auth.Controllers.Account.Read(result.Model.Profile.AccountId, identityToken);
            result.Alternatives.Add(response.Model);
        }

        // Retorna el resultado
        return result ?? new();

    }


    /// <summary>
    /// Obtiene todas las salida asociadas a un inventario
    /// </summary>
    /// <param name="id">Id del inventario</param>
    [HttpGet("all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<OutflowDataModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Reader, InventoryRoles.Supervisor];

        // Si no tienen permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await outflowRepository.ReadAll(id);

        // Retorna el resultado
        return result ?? new();
    }


    /// <summary>
    /// Actualizar la fecha de una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="date">Nueva fecha.</param>
    /// <param name="token">Token de acceso</param>
    [HttpPatch]
    [InventoryToken]
    public async Task<HttpResponseBase> Update([FromHeader] int id, [FromQuery] DateTime date, [FromHeader] string token)
    {

        // Validar parámetros.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso IamService.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Outflow,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await outflowRepository.Update(id, date);

        // Retorna el resultado
        return result ?? new();

    }


    /// <summary>
    /// Informe mensual de Salidas
    /// </summary>
    /// <param name="token">Token.</param>
    /// <param name="id">Id del inventario</param>
    /// <param name="month">Mes</param>
    /// <param name="year">Año</param>
    [HttpGet("info")]
    [InventoryToken]
    public async Task<ReadOneResponse<List<byte>>> Informe([FromHeader] string token, [FromHeader] int id, [FromHeader] int month, [FromHeader] int year)
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

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Renderizar el informe.
        await outflowReport.Render(month, year, id);

        System.IO.File.WriteAllText("wwwroot/Plantillas/Informes/Salidas/Prueba.html", outflowReport.Html);

        //if (response.File.Length <= 0)
        return new(Responses.UnavailableService);


        // Retorna el resultado
        //return new(Responses.Success, response.File.ToList());

    }

}