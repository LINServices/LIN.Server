using LIN.Inventory.Data;

namespace LIN.Inventory.Services.Reportes;

public class OutflowsReport(Outflows outflowData) : BaseReport
{

    /// <summary>
    /// Renderiza el informe de salidas.
    /// </summary>
    /// <param name="month">Mes.</param>
    /// <param name="year">Año.</param>
    /// <param name="inventoryId">Id del inventario.</param>
    public async Task Render(int month, int year, int inventoryId)
    {

        // Obtiene el informe.
        var outflows = outflowData.Informe(month, year, inventoryId);
        await outflows;

        // Obtener la plantilla base.
        Html = File.ReadAllText("wwwroot/Plantillas/Informes/Salidas/General.html");

        // Componentes.
        string rowBase = File.ReadAllText("wwwroot/Plantillas/Informes/Salidas/Row.html");

        // Variables.
        decimal utilidadTotal = 0;
        decimal gananciaTotal = 0;
        StringBuilder rows = new();

        // Recorrer.
        foreach (var row in outflows.Result.Models)
        {
            string tipo = "";
            decimal utilidad = 0;
            decimal ganancia = 0;

            switch (row.Type)
            {
                case OutflowsTypes.Consumo:
                    tipo = "Consumo Interno";
                    utilidad = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Perdida:
                    tipo = "Perdida";
                    utilidad = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Caducidad:
                    tipo = "Caducidad";
                    utilidad = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Venta:
                    tipo = "Venta";
                    utilidad = (row.PrecioVenta - row.PrecioCompra) * row.Cantidad;
                    ganancia = row.PrecioVenta * row.Cantidad;
                    break;

                case OutflowsTypes.Fraude:
                    tipo = "Fraude";
                    utilidad = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                case OutflowsTypes.Donacion:
                    tipo = "Donación";
                    utilidad = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    ganancia = Global.Utilities.Math.ToNegative(row.PrecioCompra) * row.Cantidad;
                    break;

                default:
                    continue;
            }

            // Sumar utilidad.
            utilidadTotal += utilidad;
            gananciaTotal += ganancia;

            // Organizar la fila.
            string rowElement = rowBase.Replace("@Tipo", tipo)
                                       .Replace("@Codigo", $"{row.ProductCode}")
                                       .Replace("@Nombre", $"{row.ProductName}")
                                       .Replace("@Cantidad", $"{row.Cantidad}")
                                       .Replace("@Ganancia", $"{ganancia}")
                                       .Replace("@Utilidad", $"{utilidad}");

            // Color.
            rowElement = utilidad < 0
                    ? rowElement.Replace("@Color", "red-500")
                    : utilidad == 0
                    ? rowElement.Replace("@Color", "black")
                    : rowElement.Replace("@Color", "green-600");

            // Agregar la fila.
            rows.AppendLine(rowElement);
        }

        // Reemplazar variables generales del informe.
        Html = Html.Replace("@Rows", rows.ToString())
             .Replace("@Ganancia", $"{utilidadTotal}")
             .Replace("@Date", $"{DateTime.Now:yyy.MM.dd}")
             .Replace("@Direccion", $"")
             .Replace("@Año", $"{year}");

        // Comprimir contenido HTML.
        CompressHtml();

    }


}