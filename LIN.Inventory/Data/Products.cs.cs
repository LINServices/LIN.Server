namespace LIN.Inventory.Data;


public partial class Products
{


    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="data">Modelo del producto.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<CreateResponse> Create(ProductModel data, Conexión context)
    {

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {

                // InventoryId ya existe.
                context.DataBase.Attach(data.Inventory);

                // Detalle.
                data.DetailModel!.Product = data;

                context.DataBase.Productos.Add(data);

                // Guarda los detalles
                await context.DataBase.SaveChangesAsync();

                // Finaliza
                transaction.Commit();
                return new(Responses.Success, data.Id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                context.DataBase.Remove(data);
                ServerLogger.LogError(ex.Message);
            }
        }

        return new();

    }



    /// <summary>
    /// Obtiene un producto.
    /// </summary>
    /// <param name="id">Id del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ProductModel>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var producto = await Query.Products.Read(id, context).FirstOrDefaultAsync();

            if (producto == null)
                return new(Responses.NotRows);

            return new(Responses.Success, producto);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



    /// <summary>
    /// Obtiene un producto.
    /// </summary>
    /// <param name="id">Id de el detalle</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ProductModel>> ReadByDetail(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var producto = await Query.Products.ReadByDetail(id, context).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (producto == null)
                return new(Responses.NotRows);

            return new(Responses.Success, producto);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de productos asociados a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<ProductModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var productos = await Query.Products.ReadAll(id, context).ToListAsync();

            if (productos == null)
                return new(Responses.NotRows);



            return new(Responses.Success, productos);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



    /// <summary>
    /// Actualiza la información base de un producto.
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateBase(ProductModel data, Conexión context)
    {
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {

                context.DataBase.SaveChanges();
                transaction.Commit();
                return new(Responses.Success);

            }
            catch
            {
                transaction.Rollback();
            }
        }

        return new();

    }



    /// <summary>
    /// Actualiza la información del detalle de un producto
    /// ** No actualiza las existencias
    /// </summary>
    /// <param name="id">Id del producto</param>
    /// <param name="data">Nuevo modelo</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateDetail(int id, ProductDetailModel data, Conexión context)
    {

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                // Obtiene el producto detalle antiguo
                var producto = (from PD in context.DataBase.ProductoDetalles
                                where PD.ProductId == id && PD.Estado == ProductStatements.Normal
                                select PD.Id).FirstOrDefault();

                // Si no se encuentra
                if (producto <= 0)
                {
                    transaction.Rollback();
                    return new(Responses.NotRows);
                }

                // Actualiza el antiguo detalle
                var productoDetalleAntiguo = await context.DataBase.ProductoDetalles.FindAsync(producto);

                // SI no existe el detalle
                if (productoDetalleAntiguo == null)
                {
                    transaction.Rollback();
                    return new(Responses.Undefined);
                }

                // Actualiza el estado
                productoDetalleAntiguo.Estado = ProductStatements.Deprecated;



                // Guarda los cambios
                context.DataBase.SaveChanges();
                transaction.Commit();
                return new(Responses.Success);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ServerLogger.LogError(ex.Message);
            }
        }

        return new();

    }



    /// <summary>
    /// Actualiza toda información base de un producto.
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Update(ProductModel data, Conexión context)
    {

        // Ejecución (Transacción).
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {


                // Obtiene el producto
                var producto = await context.DataBase.Productos.FindAsync(data.Id);

                // Si no se encuentra
                if (producto == null)
                {
                    transaction.Rollback();
                    return new(Responses.NotRows);
                }


                // Obtiene el producto detalle antiguo
                var productoDetailId = (from PD in context.DataBase.ProductoDetalles
                                        where PD.ProductId == producto.Id && PD.Estado == ProductStatements.Normal
                                        select PD.Id).FirstOrDefault();

                // Obtiene el antiguo detalle
                var productoDetalleAntiguo = await context.DataBase.ProductoDetalles.FindAsync(productoDetailId);

                // SI no existe el detalle
                if (productoDetalleAntiguo == null)
                {
                    transaction.Rollback();
                    return new(Responses.Undefined);
                }

                // Actualiza el estado
                productoDetalleAntiguo.Estado = ProductStatements.Deprecated;


                // Guarda los cambios
                context.DataBase.SaveChanges();

                transaction.Commit();

                return new(Responses.Success);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ServerLogger.LogError(ex.Message);
            }
        }

        return new();

    }



    /// <summary>
    /// Elimina un producto de un inventario.
    /// </summary>
    /// <param name="id">Id del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Delete(int id, Conexión context)
    {

        try
        {

            var producto = await (from P in context.DataBase.Productos
                                  where P.Id == id
                                  select P).FirstOrDefaultAsync();

            // Respuesta
            if (producto == null)
            {
                return new();
            }


            producto.Statement = ProductBaseStatements.Deleted;

            // Guarda los cambios
            context.DataBase.SaveChanges();

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();

    }



}