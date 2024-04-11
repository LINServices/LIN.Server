namespace LIN.Inventory.Data;


public partial class Outflows
{


    /// <summary>
    /// Crea una salida de inventario.
    /// </summary>
    /// <param name="data">Modelo de la salida.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<CreateResponse> Create(OutflowDataModel data, Conexión context)
    {

        data.ID = 0;

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                // Detalles.
                var details = data.Details;
                data.Details = [];

                context.DataBase.Attach(data.Inventory);

                // Entrada base
                await context.DataBase.Salidas.AddAsync(data);



                // Guarda cambios
                context.DataBase.SaveChanges();

                // Detalles
                foreach (var detail in details)
                {
                    // Modelo
                    detail.ID = 0;
                    detail.Movement = data;

                    // Agregar los detalles.
                    context.DataBase.DetallesSalidas.Add(detail);

                    context.DataBase.Attach(detail.ProductDetail);

                    if (detail.Cantidad <= 0)
                        throw new Exception("Invalid detail quantity");


                    // Detalle de un producto
                    var productoDetail = context.DataBase.ProductoDetalles.Where(T => T.Id == detail.ProductDetailId && T.Estado == ProductStatements.Normal).Select(t => new { t.Quantity }).FirstOrDefault();

                    // Si no existe el detalle
                    if (productoDetail == null)
                    {
                        ServerLogger.LogError("GRAVE - - No existe un detail");
                        throw new Exception();
                    }


                    // Nuevos datos
                    int newStock = productoDetail.Quantity - detail.Cantidad;


                    // Si el producto no tiene suficiente stock
                    if (newStock < 0)
                    {
                        return new(Responses.InvalidParam, -1, $"El producto no tiene stock suficiente");
                    }


                    await context.DataBase.ProductoDetalles.Where(t => t.Id == detail.ProductDetailId).ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity - detail.Cantidad));


                }


                // Guarda los detalles
                await context.DataBase.SaveChangesAsync();

                // Finaliza
                transaction.Commit();
                return new(Responses.Success, data.ID);
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
    /// Obtiene una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="includeDetails">Incluir los detalles.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<OutflowDataModel>> Read(int id, bool includeDetails, Conexión context)
    {

        // Ejecución
        try
        {
            // Selecciona la entrada
            var salida = context.DataBase.Salidas.FirstOrDefault(T => T.ID == id);

            if (salida == null)
            {
                return new(Responses.NotRows);
            }

            // Si es una mascara
            if (includeDetails)
            {
                salida.Details = await (from de in context.DataBase.DetallesSalidas
                                        where de.MovementId == id
                                        select new OutflowDetailsDataModel
                                        {
                                            ID = de.ID,
                                            Cantidad = de.Cantidad,
                                            MovementId = de.MovementId,
                                            ProductDetailId = de.ProductDetailId,
                                            ProductDetail = new()
                                            {
                                                Product = new()
                                                {
                                                    Name = de.ProductDetail.Product.Name,
                                                    Category = de.ProductDetail.Product.Category,
                                                    Code = de.ProductDetail.Product.Code,
                                                }
                                            }
                                        }).ToListAsync();
            }

            // Si se necesitan los detales
            else
                salida.CountDetails = context.DataBase.DetallesSalidas.Count(t => t.MovementId == id);


            // Calcular inversion.
            salida.Inversion = await (from de in context.DataBase.DetallesSalidas
                                      where de.MovementId == id
                                      select de.ProductDetail.PrecioCompra * de.Cantidad).SumAsync();

            // Calcular inversion.
            salida.Ganancia = await (from de in context.DataBase.DetallesSalidas
                                     where de.MovementId == id
                                     select de.ProductDetail.PrecioVenta * de.Cantidad).SumAsync();

            // Calcular inversion.
            salida.Utilidad = await (from de in context.DataBase.DetallesSalidas
                                     where de.MovementId == id
                                     select (de.ProductDetail.PrecioVenta - de.ProductDetail.PrecioCompra) * de.Cantidad).SumAsync();

            // Retorna
            return new(Responses.Success, salida);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de salidas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<OutflowDataModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = from S in context.DataBase.Salidas
                      where S.InventoryId == id
                      orderby S.Date descending
                      select new OutflowDataModel()
                      {
                          ID = S.ID,
                          Date = S.Date,
                          InventoryId = S.InventoryId,
                          ProfileID = S.ProfileID,
                          Type = S.Type,
                          CountDetails = context.DataBase.DetallesSalidas.Count(t => t.MovementId == S.ID)
                      };

            var lista = await res.ToListAsync();

            // Si no existe el modelo
            if (lista == null)
                return new(Responses.NotRows);

            return new(Responses.Success, lista);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



    /// <summary>
    /// Actualizar la fecha de una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="date">Nueva fecha.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> Update(int id, DateTime date, Conexión context)
    {

        // Ejecución
        try
        {

            // Update.
            var update = await (from outflow in context.DataBase.Salidas
                                where outflow.ID == id
                                select outflow).ExecuteUpdateAsync(t => t.SetProperty(t => t.Date, date));

            // Si no existe el modelo
            if (update <= 0)
                return new(Responses.NotRows);

            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



}