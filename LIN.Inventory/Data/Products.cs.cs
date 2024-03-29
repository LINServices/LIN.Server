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

        try
        {

            // Actualizar el estado anterior.
            var update = await (from PD in context.DataBase.Productos
                                where PD.Id == data.Id
                                select PD)
                                .ExecuteUpdateAsync(t => t
                                .SetProperty(t => t.Category, data.Category)
                                .SetProperty(t => t.Code, p => data.Code ?? p.Code)
                                .SetProperty(t => t.Description, p => data.Description ?? p.Description)
                                .SetProperty(t => t.Name, p => data.Name ?? p.Name)
                                .SetProperty(t => t.Image, p => data.Image ?? p.Image)
                                );

            context.DataBase.SaveChanges();

            if (update <= 0)
                return new(Responses.NotRows);

            return new(Responses.Success);

        }
        catch
        {
        }

        return new();

    }



    /// <summary>
    /// Actualiza la información del detalle de un producto
    /// ** No actualiza las existencias
    /// </summary>
    /// <param name="id">Id del producto.</param>
    /// <param name="data">Nuevo modelo de detalle.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> UpdateDetail(int id, ProductDetailModel data, Conexión context)
    {

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.CurrentTransaction ?? context.DataBase.Database.BeginTransaction())
        {
            try
            {

                data.Id = 0;

                // Modelo.
                data.Product = new()
                {
                    Id = id,
                };

                // Actualizar el estado anterior.
                var update = await (from PD in context.DataBase.ProductoDetalles
                                    where PD.ProductId == id
                                    select PD).ExecuteUpdateAsync(t => t.SetProperty(t => t.Estado, ProductStatements.Deprecated));

                // Definir producto.
                context.DataBase.Attach(data.Product);

                // Agregar.
                context.DataBase.ProductoDetalles.Add(data);

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
        using (var transaction = context.DataBase.Database.CurrentTransaction ?? context.DataBase.Database.BeginTransaction())
        {
            try
            {

                // Actualizar producto.
                var responseBase = await UpdateBase(data);

                // Validar.
                if (responseBase.Response != Responses.Success)
                {
                    transaction.Rollback();
                    return new();
                }

                // Actualizar detalle.
                if (data.DetailModel != null)
                    await UpdateDetail(data.Id, data.DetailModel);


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