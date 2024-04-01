namespace LIN.Inventory.Data;


public partial class Statistics
{


    /// <summary>
    /// Ventas de un perfil en un periodo de tiempo.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="initDate">Fecha inicial.</param>
    /// <param name="endDate">Fecha final.</param>
    public async static Task<ReadOneResponse<int>> Sales(int profile, DateTime initDate, DateTime endDate)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Sales(profile, initDate, endDate, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Ventas de un perfil en un periodo de tiempo.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="initDate">Fecha inicial.</param>
    /// <param name="endDate">Fecha final.</param>
    public async static Task<ReadAllResponse<SalesModel>> SalesOn(int profile, DateTime initDate, DateTime endDate)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await SalesOn(profile, initDate, endDate, context);
        context.CloseActions(connectionKey);
        return res;

    }



}