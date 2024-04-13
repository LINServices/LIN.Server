namespace LIN.Inventory.Controllers;


[Route("outflow")]
public class OutflowController : ControllerBase
{


    /// <summary>
    /// Nuevo movimiento de salida.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost("create")]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] OutflowDataModel modelo, [FromHeader] string token)
    {

        // Validar parámetros.
        if (!modelo.Details.Any() || modelo.Type == OutflowsTypes.None)
            return new(Responses.InvalidParam);


        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = modelo.InventoryId,
            Profile = tokenInfo.ProfileId
        });

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

        // Si no tiene el rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Establecer el modelo.
        modelo.Inventory = new()
        {
            ID = modelo.InventoryId
        };

        // Crea la nueva entrada
        var response = await Data.Outflows.Create(modelo);

        return response;

    }



    /// <summary>
    /// Obtiene una salida
    /// </summary>
    /// <param name="id">Id de la entrada</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<OutflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] string token, [FromHeader] bool includeDetails = false)
    {

        // Comprobaciones.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Outflow,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Outflows.Read(id, includeDetails);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene todas las salida asociadas a un inventario
    /// </summary>
    /// <param name="id">Id del inventario</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<OutflowDataModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles aceptados.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no tienen permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Outflows.ReadAll(id);

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

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Outflow,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Outflows.Update(id, date);

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

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Member];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };


        // Contexto.
        var context = Conexión.GetOneConnection();

        // Obtiene el informe.
        var resultTask = Data.Outflows.Informe(month, year, id, context.context);


        var inventoryTask = Data.Inventories.Read(id);


        var result = await resultTask;
        var inventory = await inventoryTask;


        decimal gananciaTotal = 0;

        string HTML = System.IO.File.ReadAllText("wwwroot/Plantillas/Informes/Salidas/General.html");

        string rows = "";
        foreach (var row in result.Models)
        {
            string tipo = "";
            decimal ganancia = 0;

            switch (row.Type)
            {

                case OutflowsTypes.None:
                    continue;

                case OutflowsTypes.Consumo:
                    tipo = "Consumo Interno";
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Perdida:
                    tipo = "Perdida";
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Caducidad:
                    tipo = "Caducidad";
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Venta:
                    tipo = "Venta";
                    ganancia = (row.PrecioVenta - row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Fraude:
                    tipo = "Fraude";
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Donacion:
                    tipo = "Donación";
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                default:
                    continue;
            }

            gananciaTotal += ganancia;

            string html = System.IO.File.ReadAllText("wwwroot/Plantillas/Informes/Salidas/Row.html"); ;

            html = html.Replace("@Tipo", $"{tipo}");
            html = html.Replace("@Codigo", $"{row.ProductCode}");
            html = html.Replace("@Nombre", $"{row.ProductName}");
            html = html.Replace("@Cantidad", $"{row.Cantidad}");
            html = html.Replace("@Ganancia", $"{ganancia}");

            if (ganancia < 0)
                html = html.Replace("@Color", "red-500");
            else if (ganancia == 0)
                html = html.Replace("@Color", "black");
            else
                html = html.Replace("@Color", "green-600");

            rows += html;
        }


        HTML = HTML.Replace("@Rows", rows);
        HTML = HTML.Replace("@Ganancia", $"{gananciaTotal}");
        HTML = HTML.Replace("@Date", $"{DateTime.Now:yyy.MM.dd}");
        //HTML = HTML.Replace("@Mes", $"{Utilities.IntToMonth(month)}");
        HTML = HTML.Replace("@Año", $"{year}");
        //HTML = HTML.Replace("@Name", $"{user.Model.Nombre}");
        HTML = HTML.Replace("@Direccion", $"{inventory.Model.Direction}");
        HTML = HTML.Replace("@Inventario", $"{inventory.Model.Nombre}");


        var x = HTML;

        //var response = await LIN.Access.Developer.Controllers.PDF.ConvertHTML(HTML);


        //if (response.File.Length <= 0)
        return new(Responses.UnavailableService);


        // Retorna el resultado
        //return new(Responses.Success, response.File.ToList());

    }

}