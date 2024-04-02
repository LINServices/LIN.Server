namespace LIN.Inventory.Controllers;


[Route("statistics")]
public class StatisticsController : Controller
{


    /// <summary>
    /// Estadísticas del home.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    [HttpGet]
    [InventoryToken]
    public async Task<HttpReadOneResponse<HomeDto>> HomeService([FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        int profile = tokenInfo.ProfileId;

        // Fecha actual.
        DateTime now = DateTime.Now;

        // Ventas de esta semana.
        var weekSales = Data.Statistics.SalesOn(profile, now.AddDays(-7), now);

        // Ventas de la semana pasada.
        var lastWeekSales = Data.Statistics.Sales(profile, now.AddDays(-14), now.AddDays(-7));

        // Ventas del dia.
        var daySales = Data.Statistics.Sales(profile, new DateTime(now.Year, now.Month, now.Day, 0, 0, 0), new DateTime(now.Year, now.Month, now.Day, 23, 59, 59));

        // Esperar las tareas.
        await Task.WhenAll([weekSales, lastWeekSales, daySales]);

        // Respuesta.
        return new ReadOneResponse<HomeDto>()
        {
            Model = new()
            {
                LastWeekSalesTotal = lastWeekSales.Result.Model,
                WeekSalesTotal = weekSales.Result.Models.Sum(T => T.Money),
                WeekSales = weekSales.Result.Models,
                TodaySalesTotal = daySales.Result.Model
            },
            Response = Responses.Success
        };

    }


}