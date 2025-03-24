namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class ProductsRepository(Context.Context context, ILogger<ProductsRepository> logger) : IProductsRepository
{

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="data">Modelo del producto.</param>
    public async Task<CreateResponse> Create(ProductModel data)
    {

        // Ejecución (Transacción)
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {

                // InventoryId ya existe.
                context.Attach(data.Inventory);

                // Detalle.
                data.Details.First().Product = data;

                context.Productos.Add(data);

                // Guarda los detalles
                await context.SaveChangesAsync();

                // Finaliza
                transaction.Commit();
                return new(Responses.Success, data.Id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                context.Remove(data);
                logger.LogWarning(ex, "Error");
            }
        }

        return new();

    }


    /// <summary>
    /// Obtiene un producto.
    /// </summary>
    /// <param name="id">Id del producto</param>
    public async Task<ReadOneResponse<ProductModel>> Read(int id)
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
            logger.LogWarning(ex, "Error");
        }
        return new();
    }


    /// <summary>
    /// Obtiene un producto.
    /// </summary>
    /// <param name="id">Id de el detalle</param>
    public async Task<ReadOneResponse<ProductModel>> ReadByDetail(int id)
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
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtiene la lista de productos asociados a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    public async Task<ReadAllResponse<ProductModel>> ReadAll(int id)
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
            logger.LogWarning(ex, "Error");
        }
        return new();
    }


    /// <summary>
    /// Actualiza la información base de un producto.
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async Task<ResponseBase> UpdateBase(ProductModel data)
    {

        try
        {

            // Actualizar el estado anterior.
            var update = await (from PD in context.Productos
                                where PD.Id == data.Id
                                select PD)
                                .ExecuteUpdateAsync(t => t
                                .SetProperty(t => t.Category, data.Category)
                                .SetProperty(t => t.Code, p => data.Code ?? p.Code)
                                .SetProperty(t => t.Description, p => data.Description ?? p.Description)
                                .SetProperty(t => t.Name, p => data.Name ?? p.Name)
                                .SetProperty(t => t.Image, p => data.Image ?? p.Image)
                                );

            context.SaveChanges();

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
    public async Task<ResponseBase> UpdateDetail(int id, ProductDetailModel data)
    {

        // Ejecución (Transacción)
        using (var transaction = context.Database.CurrentTransaction ?? context.Database.BeginTransaction())
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
                var update = await (from PD in context.ProductoDetalles
                                    where PD.ProductId == id
                                    select PD).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, ProductStatements.Deprecated));

                // Definir producto.
                context.Attach(data.Product);

                // Agregar.
                context.ProductoDetalles.Add(data);

                // Guarda los cambios
                context.SaveChanges();
                transaction.Commit();
                return new(Responses.Success);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.LogWarning(ex, "Error");
            }
        }

        return new();

    }


    /// <summary>
    /// Actualiza toda información base de un producto.
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async Task<ResponseBase> Update(ProductModel data)
    {

        // Ejecución (Transacción).
        using (var transaction = context.Database.CurrentTransaction ?? context.Database.BeginTransaction())
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
                if (data.Details.Any())
                    await UpdateDetail(data.Id, data.Details[0]);


                // Guarda los cambios
                context.SaveChanges();

                transaction.Commit();

                return new(Responses.Success);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.LogWarning(ex, "Error");
            }
        }

        return new();

    }


    /// <summary>
    /// Elimina un producto de un inventario.
    /// </summary>
    /// <param name="id">Id del producto</param>
    public async Task<ResponseBase> Delete(int id)
    {

        try
        {

            var producto = await (from P in context.Productos
                                  where P.Id == id
                                  select P).FirstOrDefaultAsync();

            // Respuesta
            if (producto == null)
            {
                return new();
            }


            producto.Statement = ProductBaseStatements.Deleted;

            // Guarda los cambios
            context.SaveChanges();

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();

    }


}