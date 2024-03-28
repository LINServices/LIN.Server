namespace LIN.Inventory.Controllers;


[Route("inflow")]
public class InflowController : ControllerBase
{


    /// <summary>
    /// Crear nueva movimiento de entrada.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost("create")]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] InflowDataModel modelo, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Establecer el perfil.
        modelo.ProfileID = tokenInfo.ProfileId;

        // Comprobaciones
        if (!modelo.Details.Any() || modelo.Type == InflowsTypes.Undefined)
            return new(Responses.InvalidParam);

        // Acceso Iam.
        var iam = await Iam.OnInventory(modelo.InventoryId, tokenInfo.ProfileId);

        // Roles que pueden crear.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

        // Si no tiene ese rol.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Generar el modelo.
        modelo.Inventory = new()
        {
            ID = modelo.InventoryId
        };

        // Crea la nueva entrada.
        var response = await Data.Inflows.Create(modelo);

        // Respuesta.
        return response;

    }



    /// <summary>
    /// Obtener el movimiento (entrada).
    /// </summary>
    /// <param name="id">Id de la entrada.</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<InflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] string token, [FromHeader] bool includeDetails = false)
    {

        // Validar parámetros.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener el inventario.
        var inventory = await Data.Inventories.FindByInflow(id);

        // Si hubo un error.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el movimiento.",
                Response = Responses.Unauthorized
            };


        // Acceso Iam.
        var iam = await Iam.OnInventory(inventory.Model, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Inflows.Read(id, includeDetails);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene las entradas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<InflowDataModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Validar parámetros.
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.OnInventory(id, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no cumple con los roles.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Obtiene el usuario
        var result = await Data.Inflows.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Informe mensual de entradas
    /// </summary>
    /// <param name="contextUser">Id del usuario que hace la petición</param>
    /// <param name="id">Id del inventario</param>
    /// <param name="month">Mes</param>
    /// <param name="year">Año</param>
    [HttpGet("info")]
    [Obsolete("Este método esta en desuso")]
    public async Task<ReadOneResponse<List<byte>>> Informe([FromHeader] int contextUser, [FromHeader] int id, [FromHeader] int month, [FromHeader] int year)
    {

        if (id <= 0 || contextUser <= 0)
            return new();

        var context = Conexión.GetOneConnection();

        // Obtiene el usuario
        var resultTask = Data.Inflows.Informe(month, year, id, context.context);
        var userTask = Data.Profiles.Read(contextUser);
        var inventoryTask = Data.Inventories.Read(id);


        var result = await resultTask;
        var user = await userTask;
        var inventory = await inventoryTask;


        decimal inversionFinal = 0;
        decimal posibleGananciasFinal = 0;

        string HTML = System.IO.File.ReadAllText("wwwroot/Plantillas/Informes/Entradas/General.html");

        string rows = "";
        foreach (var row in result.Models)
        {
            string tipo = "";
            decimal inversion = 0;
            decimal posibleGanancias = 0;

            switch (row.Type)
            {

                case InflowsTypes.Undefined:
                    continue;

                case InflowsTypes.Compra:
                    tipo = "Compra";
                    inversion += row.PrecioCompra * row.Cantidad;
                    posibleGanancias += (row.PrecioVenta - row.PrecioCompra) * row.Cantidad;
                    break;

                case InflowsTypes.Devolucion:
                    tipo = "Devolucion";
                    break;

                case InflowsTypes.Regalo:
                    tipo = "Regalo";
                    posibleGanancias += row.PrecioVenta;
                    break;

                case InflowsTypes.Ajuste:
                    tipo = "Ajuste";
                    break;

            }

            inversionFinal += inversion;
            posibleGananciasFinal += posibleGanancias;

            string html = System.IO.File.ReadAllText("wwwroot/Plantillas/Informes/Entradas/Row.html"); ;

            html = html.Replace("@Tipo", $"{tipo}");
            html = html.Replace("@Codigo", $"{row.ProductCode}");
            html = html.Replace("@Nombre", $"{row.ProductName}");
            html = html.Replace("@Cantidad", $"{row.Cantidad}");
            html = html.Replace("@Inversion", $"{inversion}");
            html = html.Replace("@Ganancia", $"{posibleGanancias}");


            rows += html;
        }


        HTML = HTML.Replace("@Rows", rows);
        HTML = HTML.Replace("@Inversion", $"{inversionFinal}");
        HTML = HTML.Replace("@Ganancia", $"{posibleGananciasFinal}");
        HTML = HTML.Replace("@Date", $"{DateTime.Now:yyy.MM.dd}");
        //HTML = HTML.Replace("@Mes", $"{Utilities.IntToMonth(month)}");
        HTML = HTML.Replace("@Año", $"{year}");
        //  HTML = HTML.Replace("@Name", $"{user.Model.Nombre}");
        HTML = HTML.Replace("@Direccion", $"{inventory.Model.Direction}");
        HTML = HTML.Replace("@Inventario", $"{inventory.Model.Nombre}");



        // var response = await LIN.Access.Developer.Controllers.PDF.ConvertHTML(HTML);


        // if (response.File.Length <= 0)
        return new(Responses.UnavailableService);


        // Retorna el resultado
        //return new(Responses.Success, response.File.ToList());

    }



}