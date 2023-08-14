using LIN.Inventory.Data;

namespace LIN.Inventory.Controllers;


[Route("inflow")]
public class InflowController : ControllerBase
{


    /// <summary>
    /// Crea una nueva entrada
    /// </summary>
    /// <param name="modelo">modelo de la entrada</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] InflowDataModel modelo)
    {

        // Comprobaciones
        if (!modelo.Details.Any() || modelo.Type == InflowsTypes.Undefined)
            return new(Responses.InvalidParam);

        // Crea la nueva entrada
        var response = await Inflows.Create(modelo);

        return response;

    }



    /// <summary>
    /// Obtiene una entrada
    /// </summary>
    /// <param name="id">ID de la entrada</param>
    /// <param name="mascara">TRUE si NO necesita los detalles, y FALSE si necesita los detalles</param>
    [HttpGet("read")]
    public async Task<HttpReadOneResponse<InflowDataModel>> ReadOne([FromHeader] int id, [FromHeader] bool mascara = false)
    {

        // Comprobación
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Inflows.Read(id, mascara);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene las entradas asociadas a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<InflowDataModel>> ReadAll([FromHeader] int id)
    {

        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Inflows.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Informe mensual de entradas
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
        var resultTask = Inflows.Informe(month, year, id, context.context);
        var userTask = Profiles.Read(contextUser);
        var inventoryTask = Inventories.Read(id);


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
        HTML = HTML.Replace("@Direccion", $"{inventory.Model.Direccion}");
        HTML = HTML.Replace("@Inventario", $"{inventory.Model.Nombre}");



       // var response = await LIN.Access.Developer.Controllers.PDF.ConvertHTML(HTML);


       // if (response.File.Length <= 0)
            return new(Responses.UnavailableService);


        // Retorna el resultado
        //return new(Responses.Success, response.File.ToList());

    }



}