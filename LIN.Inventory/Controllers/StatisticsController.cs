namespace LIN.Inventory.Controllers;

[Route("[Controller]")]
[RateLimit(requestLimit: 5, timeWindowSeconds: 60, blockDurationSeconds: 150)]
public class StatisticsController(Persistence.Data.Statistics statisticsData) : Controller
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

        // Id del perfil.
        int profile = tokenInfo.ProfileId;

        // Fecha actual.
        DateTime now = DateTime.Now;

        // Ventas de esta semana.
        var weekSales = statisticsData.SalesOn(profile, now.AddDays(-7), now);

        // Ventas de la semana pasada.
        var lastWeekSales = statisticsData.Sales(profile, now.AddDays(-14), now.AddDays(-7));

        // Ventas del dia.
        var daySales = statisticsData.Sales(profile, new DateTime(now.Year, now.Month, now.Day, 0, 0, 0), new DateTime(now.Year, now.Month, now.Day, 23, 59, 59));

        // Ventas del dia anterior.
        var lastDaySales = statisticsData.Sales(profile, new DateTime(now.Year, now.Month, now.Day - 1, 0, 0, 0), new DateTime(now.Year, now.Month, now.Day - 1, 23, 59, 59));

        // Esperar las tareas.
        await Task.WhenAll([weekSales, lastWeekSales, daySales, lastDaySales]);

        // Respuesta.
        return new ReadOneResponse<HomeDto>()
        {
            Model = new()
            {
                LastWeekSalesTotal = lastWeekSales.Result.Model,
                WeekSalesTotal = weekSales.Result.Models.Sum(T => T.Money),
                WeekSales = weekSales.Result.Models.ToList(),
                TodaySalesTotal = daySales.Result.Model,
                YesterdaySalesTotal = lastDaySales.Result.Model
            },
            Response = Responses.Success
        };

    }

}