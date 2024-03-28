namespace LIN.Inventory.Data;


public partial class Products
{


    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async static Task<CreateResponse> Create(ProductModel data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un producto.
    /// </summary>
    /// <param name="id">Id del producto</param>
    public async static Task<ReadOneResponse<ProductModel>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un producto.
    /// </summary>
    /// <param name="id">Id de el detalle</param>
    public async static Task<ReadOneResponse<ProductModel>> ReadByDetail(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadByDetail(id, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene la lista de productos asociados a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    public async static Task<ReadAllResponse<ProductModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Actualiza la información base de un producto.
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async static Task<ResponseBase> UpdateBase(ProductModel data)
    {
        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await UpdateBase(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Actualiza la información del detalle de un producto.
    /// </summary>
    /// <param name="id">Id del producto.</param>
    /// <param name="data">Nuevo modelo.</param>
    public async static Task<ResponseBase> UpdateDetail(int id, ProductDetailModel data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await UpdateDetail(id, data, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Actualiza toda información base de un producto.
    /// </summary>
    /// <param name="data">Modelo del producto.</param>
    public async static Task<ResponseBase> Update(ProductModel data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Update(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Elimina un producto de un inventario.
    /// </summary>
    /// <param name="id">Id del producto</param>
    public async static Task<ResponseBase> Delete(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Delete(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



}