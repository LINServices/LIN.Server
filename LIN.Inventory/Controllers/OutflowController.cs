using LIN.Inventory.Data;

namespace LIN.Inventory.Controllers;


[Route("outflow")]
public class OutflowController : ControllerBase
{


    /// <summary>
    /// Crea una nueva salida
    /// </summary>
    /// <param name="modelo">Modelo la salida</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] OutflowDataModel modelo)
    {

        if (!modelo.Details.Any() || modelo.Type == OutflowsTypes.None)
            return new(Responses.InvalidParam);

        // Crea la nueva entrada
        var response = await Outflows.Create(modelo);

        return response;

    }



    /// <summary>
    /// Obtiene una salida
    /// </summary>
    /// <param name="id">ID de la entrada</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles</param>
    [HttpGet("read")]
    public async Task<HttpReadOneResponse<OutflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] bool mascara = false)
    {

        // comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Outflows.Read(id, mascara);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene todas las salida asociadas a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<OutflowDataModel>> ReadAll([FromHeader] int id)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Outflows.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }







    /// <summary>
    /// Informe mensual de Salidas
    /// </summary>
    /// <param name="contextUser">ID del usuario que hace la peticion</param>
    /// <param name="id">ID del inventario</param>
    /// <param name="month">Mes</param>
    /// <param name="year">Año</param>
    [HttpGet("info")]
    public async Task<ReadOneResponse<List<byte>>> Informe([FromHeader] int contextUser, [FromHeader] int id, [FromHeader] int month, [FromHeader] int year)
    {

        if (id <= 0 || contextUser <= 0)
            return new();

        var context = Conexión.GetOneConnection();

        // Obtiene el usuario
        var resultTask = Outflows.Informe(month, year, id, context.context);
        var userTask = Profiles.Read(contextUser);
        var inventoryTask = Inventories.Read(id);


        var result = await resultTask;
        var user = await userTask;
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
                    ganancia = Pricing.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Perdida:
                    tipo = "Perdida";
                    ganancia = Pricing.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Caducidad:
                    tipo = "Caducidad";
                    ganancia = Pricing.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Venta:
                    tipo = "Venta";
                    ganancia = (row.PrecioVenta - row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Fraude:
                    tipo = "Fraude";
                    ganancia = Pricing.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Donacion:
                    tipo = "Donación";
                    ganancia = Pricing.ToNegative(row.PrecioCompra) * row.Cantidad;
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
        HTML = HTML.Replace("@Mes", $"{Utilities.IntToMonth(month)}");
        HTML = HTML.Replace("@Año", $"{year}");
        HTML = HTML.Replace("@Name", $"{user.Model.Nombre}");
        HTML = HTML.Replace("@Direccion", $"{inventory.Model.Direccion}");
        HTML = HTML.Replace("@Inventario", $"{inventory.Model.Nombre}");



        var response = await LIN.Access.Developer.Controllers.PDF.ConvertHTML(HTML);


        if (response.File.Length <= 0)
            return new(Responses.UnavailableService);


        // Retorna el resultado
        return new(Responses.Success, response.File.ToList());

    }





}