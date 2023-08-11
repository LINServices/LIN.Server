using LIN.Inventory.Services;

namespace LIN.Inventory.Data;


public static class Products
{


    #region Abstracciones



    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async static Task<CreateResponse> Create(ProductDataTransfer data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    public async static Task<ReadOneResponse<ProductDataTransfer>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un producto
    /// </summary>
    /// <param name="id">ID de el detalle</param>
    public async static Task<ReadOneResponse<ProductDataTransfer>> ReadByDetail(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadByDetail(id, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene la lista de productos asociados a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    public async static Task<ReadAllResponse<ProductDataTransfer>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Actualiza la información base de un producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async static Task<ResponseBase> UpdateBase(ProductDataTransfer data)
    {
        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await UpdateBase(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Actualiza la información del detalle de un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="data">Nuevo modelo</param>
    public async static Task<ResponseBase> UpdateDetail(int id, ProductDetailDataModel data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await UpdateDetail(id, data, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Actualiza toda información base de un producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    public async static Task<ResponseBase> Update(ProductDataTransfer data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Update(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Elimina un producto de un inventario
    /// </summary>
    /// <param name="id">ID del producto</param>
    public async static Task<ResponseBase> Delete(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Delete(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    #endregion



    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(ProductDataTransfer data, Conexión context)
    {

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                // Plantilla
                if (data.Plantilla > 0)
                {
                    // Busca la plantilla
                    var plantilla = await (from P in context.DataBase.PlantillaProductos
                                           where P.ID == data.Plantilla
                                           select P).FirstOrDefaultAsync();

                    // Valida la plantilla
                    if (plantilla == null)
                    {
                        transaction.Rollback();
                        return new(Responses.InvalidParam);
                    }

                    // Rellena los datos
                    data.Image = plantilla.Image;
                    data.Name = plantilla.Name;
                    data.Description = plantilla.Description;
                    data.Code = plantilla.Code;
                    data.Category = plantilla.Category;

                }
                else
                {

                    // Plantilla nueva
                    var plantilla = new DBModels.ProductTemplateTable
                    {
                        Category = data.Category,
                        Name = data.Name,
                        Image = data.Image,
                        Description = data.Description,
                        Code = data.Code
                    };

                    // Respuesta
                    var response = await ProductTemplate.Create(plantilla, context);

                    data.Plantilla = response.LastID;
                }



                // Modelo de producto
                var producto = new DBModels.ProductoTable
                {
                    Estado = ProductBaseStatements.Normal,
                    Inventory = data.Inventory,
                    Plantilla = data.Plantilla,
                    Provider = data.Provider,
                };

                // Producto base
                context.DataBase.Productos.Add(producto);

                // Guarda cambios
                var taskProducto = context.DataBase.SaveChangesAsync();

                // Preparación del modelo detail
                var detail = new DBModels.ProductoDetailTable
                {
                    ID = 0,
                    Estado = ProductStatements.Normal,
                    PrecioCompra = data.PrecioCompra,
                    PrecioVenta = data.PrecioVenta,
                    Quantity = data.Quantity
                };


                await taskProducto;

                // Detalles
                detail.ProductoFK = producto.ID;
                context.DataBase.ProductoDetalles.Add(detail);

                // Guarda los detalles
                await context.DataBase.SaveChangesAsync();

                // Finaliza
                transaction.Commit();
                return new(Responses.Success, producto.ID);
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
    /// Obtiene un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ProductDataTransfer>> Read(int id, Conexión context)
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
    /// Obtiene un producto
    /// </summary>
    /// <param name="id">ID de el detalle</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ProductDataTransfer>> ReadByDetail(int id, Conexión context)
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
    /// Obtiene la lista de productos asociados a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<ProductDataTransfer>> ReadAll(int id, Conexión context)
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
    /// Actualiza la información base de un producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateBase(ProductDataTransfer data, Conexión context)
    {
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                // Nuevos modelos
                var plantilla = new DBModels.ProductTemplateTable
                {
                    ID = data.Plantilla,
                    Category = data.Category,
                    Name = data.Name,
                    Image = data.Image,
                    Description = data.Description,
                    Code = data.Code
                };

                // Cuantos productos asociados
                var count = await ProductTemplate.HasProducts(data.Plantilla, context);

                // Respuesta
                if (count.Response != Responses.Success)
                {
                    return new();
                }

                // Si no hay o solo existe 1
                if (count.Model <= 1)
                {
                    ResponseBase update = await ProductTemplate.Update(plantilla, context);

                    if (update.Response != Responses.Success)
                    {
                        transaction.Rollback();
                        return new();
                    }
                }

                // Si hay mas de uno
                else
                {

                    var create = await ProductTemplate.Create(plantilla, context);

                    if (create.Response != Responses.Success)
                    {
                        transaction.Rollback();
                        return new();
                    }

                    plantilla.ID = create.LastID;


                    // Obtiene el producto
                    var producto = await context.DataBase.Productos.FindAsync(data.ProductID);

                    // Si no se encuentra
                    if (producto == null)
                    {
                        transaction.Rollback();
                        return new(Responses.NotRows);
                    }


                    producto.Plantilla = plantilla.ID;

                }


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
    /// <param name="id">ID del producto</param>
    /// <param name="data">Nuevo modelo</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateDetail(int id, ProductDetailDataModel data, Conexión context)
    {

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                // Obtiene el producto detalle antiguo
                var producto = (from PD in context.DataBase.ProductoDetalles
                                where PD.ProductoFK == id && PD.Estado == ProductStatements.Normal
                                select PD.ID).FirstOrDefault();

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

                // Crea el nuevo estado
                var detail = new DBModels.ProductoDetailTable
                {
                    Estado = ProductStatements.Normal,
                    PrecioCompra = data.PrecioCompra,
                    PrecioVenta = data.PrecioVenta,
                    ProductoFK = productoDetalleAntiguo.ProductoFK,
                    Quantity = productoDetalleAntiguo.Quantity
                };



                // Agrega el cambio
                context.DataBase.ProductoDetalles.Add(detail);

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
    /// Actualiza toda información base de un producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Update(ProductDataTransfer data, Conexión context)
    {

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {

                // Nuevos modelos
                var plantilla = new DBModels.ProductTemplateTable
                {
                    ID = data.Plantilla,
                    Category = data.Category,
                    Name = data.Name,
                    Image = data.Image,
                    Description = data.Description,
                    Code = data.Code
                };

                // Cuantos productos asociados
                var count = await ProductTemplate.HasProducts(data.Plantilla, context);

                // Respuesta
                if (count.Response != Responses.Success)
                {
                    transaction.Rollback();
                    return new();
                }

                // Si no hay o solo existe 1
                if (count.Model <= 1)
                {
                    ResponseBase update = await ProductTemplate.Update(plantilla, context);

                    if (update.Response != Responses.Success)
                    {
                        transaction.Rollback();
                        return new();
                    }
                }

                // Si hay mas de uno
                else
                {

                    var create = await ProductTemplate.Create(plantilla, context);

                    if (create.Response != Responses.Success)
                    {
                        transaction.Rollback();
                        return new();
                    }

                    plantilla.ID = create.LastID;

                }


                // Obtiene el producto
                var producto = await context.DataBase.Productos.FindAsync(data.ProductID);

                // Si no se encuentra
                if (producto == null)
                {
                    transaction.Rollback();
                    return new(Responses.NotRows);
                }


                producto.Plantilla = plantilla.ID;


                // Obtiene el producto detalle antiguo
                var productoDetailId = (from PD in context.DataBase.ProductoDetalles
                                        where PD.ProductoFK == producto.ID && PD.Estado == ProductStatements.Normal
                                        select PD.ID).FirstOrDefault();

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

                // detalle nuevo
                var newDetail = new DBModels.ProductoDetailTable
                {
                    ID = 0,
                    Estado = ProductStatements.Normal,
                    PrecioCompra = data.PrecioCompra,
                    PrecioVenta = data.PrecioVenta,
                    ProductoFK = producto.ID,
                    Quantity = productoDetalleAntiguo.Quantity
                };


                // Agrega el cambio
                context.DataBase.ProductoDetalles.Add(newDetail);

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
    /// Elimina un producto de un inventario
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Delete(int id, Conexión context)
    {

        try
        {

            var producto = await (from P in context.DataBase.Productos
                                  where P.ID == id
                                  select P).FirstOrDefaultAsync();

            // Respuesta
            if (producto == null)
            {
                return new();
            }


            producto.Estado = ProductBaseStatements.Deleted;

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