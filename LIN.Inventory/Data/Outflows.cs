namespace LIN.Inventory.Data;


public partial class Outflows
{


    /// <summary>
    /// Crear nueva salida.
    /// </summary>
    /// <param name="data">Modelo.</param>
    public async static Task<CreateResponse> Create(OutflowDataModel data)
    {
        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="includeDetails">Incluir detalles.</param>
    public async static Task<ReadOneResponse<OutflowDataModel>> Read(int id, bool includeDetails = false)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(id, includeDetails, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene la lista de salidas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    public async static Task<ReadAllResponse<OutflowDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return res;
    }















    /// <summary>
    /// Obtiene las ventas realizadas por un usuario en todos los inventarios
    /// </summary>
    public async static Task<ReadOneResponse<int>> VentasOf(int id, int days)
    {

        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await VentasOf(id, days, context);
        context.CloseActions(connectionKey);
        return res;

    }

    public async static Task<ReadAllResponse<SalesModel>> Ventas(int id, int days)
    {

        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await Ventas(id, days, context);
        context.CloseActions(connectionKey);
        return res;

    }



}