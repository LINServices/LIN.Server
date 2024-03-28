namespace LIN.Inventory.Data;


public partial class Inflows
{


    /// <summary>
    /// Crear nueva entrada.
    /// </summary>
    /// <param name="data">Modelo.</param>
    public async static Task<CreateResponse> Create(InflowDataModel data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();


        var response = await Create(data, context);
        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtener una entrada.
    /// </summary>
    /// <param name="id">Id de la entrada.</param>
    /// <param name="includeDetails">Incluir los detalles.</param>
    public async static Task<ReadOneResponse<InflowDataModel>> Read(int id, bool includeDetails = false)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Read(id, includeDetails, context);
        context.CloseActions(connectionKey);
        return response;
    }



    /// <summary>
    /// Obtiene la lista de entradas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    public async static Task<ReadAllResponse<InflowDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return response;
    }



}