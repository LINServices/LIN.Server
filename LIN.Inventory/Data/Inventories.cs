namespace LIN.Inventory.Data;


public partial class Inventories
{


    /// <summary>
    /// Crear nuevo inventario.
    /// </summary>
    /// <param name="data">Modelo.</param>
    public async static Task<CreateResponse> Create(InventoryDataModel data)
    {

        // Conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Create(data, context);
        context.CloseActions(connectionKey);
        return response;
    }



    /// <summary>
    /// Obtiene un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    public async static Task<ReadOneResponse<InventoryDataModel>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Read(id, context);
        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene la lista de inventarios asociados a un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async static Task<ReadAllResponse<InventoryDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return response;

    }



    /// <summary>
    /// Actualizar la información de un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="name">Nuevo nombre.</param>
    /// <param name="description">Nueva descripción.</param>
    public async static Task<ResponseBase> Update(int id, string name, string description)
    {

        // Conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Update(id, name, description, context);
        context.CloseActions(connectionKey);
        return response;
    }



    /// <summary>
    /// Obtener el Id de un inventario.
    /// </summary>
    /// <param name="id">Id del producto.</param>
    public async static Task<ReadOneResponse<int>> FindByProduct(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await FindByProduct(id, context);
        context.CloseActions(connectionKey);
        return response;
    }



    /// <summary>
    /// Obtener el Id de un inventario.
    /// </summary>
    /// <param name="id">Id del detalle de producto.</param>
    public async static Task<ReadOneResponse<int>> FindByProductDetail(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await FindByProductDetail(id, context);
        context.CloseActions(connectionKey);
        return response;
    }



}