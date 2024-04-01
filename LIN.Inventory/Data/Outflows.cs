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
    /// Actualizar la fecha de una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="date">Nueva fecha.</param>
    public async static Task<ResponseBase> Update(int id, DateTime date)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Update(id, date, context);
        context.CloseActions(connectionKey);
        return response;
    }



}