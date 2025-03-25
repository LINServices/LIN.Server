namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class OutflowsRepository(Context.Context context, ILogger<OutflowsRepository> logger, IInflowsRepository inflows) : IOutflowsRepository
{

    /// <summary>
    /// Crea una salida de inventario.
    /// </summary>
    /// <param name="data">Modelo de la salida.</param>
    public async Task<CreateResponse> Create(OutflowDataModel data, bool updateInventory = true)
    {

        data.Id = 0;
        data.InflowRelatedId = null;
        if (data.ProfileId.HasValue && data.ProfileId > 0)
            data.Profile = new() { Id = data.ProfileId.Value };
        else
        {
            data.Profile = null;
            data.ProfileId = null;
        }


        // Ejecución (Transacción)
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                // Detalles.
                var details = data.Details;
                data.Details = [];

                data.Inventory = context.AttachOrUpdate(data.Inventory);

                if (data.Profile is not null)
                    data.Profile = context.AttachOrUpdate(data.Profile);

                if (data.Outsider is not null)
                    data.Outsider = context.AttachOrUpdate(data.Outsider);

                if (data.InflowRelated is not null)
                    data.InflowRelated = context.AttachOrUpdate(data.InflowRelated);

                // Entrada base
                await context.Salidas.AddAsync(data);

                // Guarda cambios
                context.SaveChanges();

                // Detalles
                foreach (var detail in details)
                {
                    // Modelo
                    detail.Id = 0;
                    detail.Movement = data;

                    detail.ProductDetail = context.AttachOrUpdate(detail.ProductDetail);

                    // Agregar los detalles.
                    context.DetallesSalidas.Add(detail);

                    if (detail.Quantity <= 0)
                        throw new Exception("Invalid detail quantity");

                    // Detalle de un producto
                    var productoDetail = context.ProductoDetalles.Where(T => T.Id == detail.ProductDetailId && T.Status == ProductStatements.Normal).Select(t => new { t.Quantity }).FirstOrDefault();

                    // Si no existe el detalle
                    if (productoDetail == null)
                    {
                        logger.LogWarning("No existe el detalle {detalle}", detail.Id);
                        throw new Exception();
                    }

                    // Nuevos datos
                    int newStock = productoDetail.Quantity - detail.Quantity;

                    // Si el producto no tiene suficiente stock
                    if (newStock < 0)
                    {
                        return new(Responses.InvalidParam, -1, $"El producto no tiene stock suficiente");
                    }

                    // Si se debe actualizar el stock del inventario.
                    if (updateInventory)
                        await context.ProductoDetalles.Where(t => t.Id == detail.ProductDetailId).ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity - detail.Quantity));

                }

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
    /// Obtiene una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="includeDetails">Incluir los detalles.</param>
    public async Task<ReadOneResponse<OutflowDataModel>> Read(int id, bool includeDetails)
    {

        // Ejecución
        try
        {
            // Selecciona la entrada
            var salida = await (from ss in context.Salidas
                                where ss.Id == id
                                select new OutflowDataModel
                                {
                                    Id = ss.Id,
                                    Status = ss.Status,
                                    Date = ss.Date,
                                    InflowRelatedId = ss.InflowRelatedId,
                                    InventoryId = ss.InventoryId,
                                    OrderId = ss.OrderId,
                                    ProfileId = ss.ProfileId,
                                    Outsider = ss.Outsider == null ? null : new()
                                    {
                                        Document = ss.Outsider.Document,
                                        Id = ss.Outsider.Id,
                                        Name = ss.Outsider.Name,
                                        Type = ss.Outsider.Type
                                    },
                                    Profile = ss.Profile != null ? new()
                                    {
                                        AccountId = ss.Profile.AccountId
                                    } : null,
                                    Type = ss.Type
                                }).FirstOrDefaultAsync();

            if (salida == null)
            {
                return new(Responses.NotRows);
            }

            // Si es una mascara
            if (includeDetails)
            {
                salida.Details = await (from de in context.DetallesSalidas
                                        where de.MovementId == id
                                        select new OutflowDetailsDataModel
                                        {
                                            Id = de.Id,
                                            Quantity = de.Quantity,
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
                salida.CountDetails = context.DetallesSalidas.Count(t => t.MovementId == id);


            // Calcular inversion.
            salida.Inversion = await (from de in context.DetallesSalidas
                                      where de.MovementId == id
                                      select de.ProductDetail.PurchasePrice * de.Quantity).SumAsync();

            // Calcular ganancia / perdida.
            salida.Ganancia = await (from de in context.DetallesSalidas
                                     where de.MovementId == id
                                     select de.Movement.Type == OutflowsTypes.Purchase
                                     ? de.ProductDetail.SalePrice * de.Quantity
                                     : -de.ProductDetail.PurchasePrice * de.Quantity).SumAsync();

            // Calcular utilidad.
            salida.Utilidad = await (from de in context.DetallesSalidas
                                     where de.Movement.Type == OutflowsTypes.Purchase
                                     where de.MovementId == id
                                     select (de.ProductDetail.SalePrice - de.ProductDetail.PurchasePrice) * de.Quantity).SumAsync();

            // Retorna
            return new(Responses.Success, salida);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtiene la lista de salidas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    public async Task<ReadAllResponse<OutflowDataModel>> ReadAll(int id)
    {

        // Ejecución
        try
        {

            var res = from S in context.Salidas
                      where S.InventoryId == id
                      orderby S.Date descending
                      select new OutflowDataModel()
                      {
                          Id = S.Id,
                          Date = S.Date,
                          InventoryId = S.InventoryId,
                          ProfileId = S.ProfileId,
                          Type = S.Type,
                          InflowRelatedId = S.InflowRelatedId,
                          Status = S.Status,
                          OrderId = S.OrderId,
                          Profile = S.Profile != null ? new()
                          {
                              Id = S.Profile.Id,
                              AccountId = S.Profile.AccountId
                          } : null,
                          CountDetails = context.DetallesSalidas.Count(t => t.MovementId == S.Id)
                      };

            var lista = await res.ToListAsync();

            // Si no existe el modelo
            if (lista == null)
                return new(Responses.NotRows);

            return new(Responses.Success, lista);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }
        return new();
    }


    /// <summary>
    /// Actualizar la fecha de una salida.
    /// </summary>
    /// <param name="id">Id de la salida.</param>
    /// <param name="date">Nueva fecha.</param>
    public async Task<ResponseBase> Update(int id, DateTime date)
    {

        // Ejecución
        try
        {

            // Update.
            var update = await (from outflow in context.Salidas
                                where outflow.Id == id
                                select outflow).ExecuteUpdateAsync(t => t.SetProperty(t => t.Date, date));

            // Si no existe el modelo
            if (update <= 0)
                return new(Responses.NotRows);

            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }
        return new();
    }


    /// <summary>
    /// Informe de un mes.
    /// </summary>
    /// <param name="month">Mes.</param>
    /// <param name="year">Año.</param>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadAllResponse<OutflowRow>> Informe(int month, int year, int inventory)
    {

        // Ejecución
        try
        {


            // Consulta.
            var query = from E in context.Salidas
                        where E.InventoryId == inventory
                        && E.Date.Year == year && E.Date.Month == month

                        join ED in context.DetallesSalidas
                        on E.Id equals ED.MovementId

                        join P in context.ProductoDetalles
                        on ED.ProductDetailId equals P.Id
                        select new OutflowRow
                        {
                            ProductId = P.ProductId,
                            PrecioCompra = P.PurchasePrice,
                            PrecioVenta = P.SalePrice,
                            Fecha = E.Date,
                            ProductCode = P.Product.Code,
                            ProductName = P.Product.Name,
                            Cantidad = ED.Quantity,
                            Type = E.Type
                        };


            var models = await query.ToListAsync();

            // Retorna
            return new(Responses.Success, models);

        }
        catch (Exception)
        {
        }

        return new();
    }


    public async Task<CreateResponse> Reverse(int order)
    {
        // Ejecución
        try
        {

            // reversar las ordenes.
            var qq = await (from ss in context.Salidas
                            where ss.OrderId == order
                            && ss.Status != MovementStatus.Reversed
                            select ss).ExecuteUpdateAsync(t => t.SetProperty(a => a.Status, MovementStatus.Reversed));

            // Ya fue reversado, o esta en proceso.
            if (qq <= 0)
                return new();


            var outflow = await (from ss in context.Salidas
                                 where ss.OrderId == order
                                 select ss).Include(t => t.Inventory).FirstOrDefaultAsync();


            var outflowDetails = await (from ss in context.Salidas
                                        where ss.OrderId == order
                                        select ss.Details).FirstOrDefaultAsync();


            // Agregar productos al inventario como Entrada Pendiente.
            var inflow = new InflowDataModel()
            {

                Status = MovementStatus.Accepted,
                Date = DateTime.Now,
                Type = InflowsTypes.Refund,
                Inventory = outflow?.Inventory!,
                Profile = null,
                IsAccepted = false,
                OutflowRelated = outflow,
                Details = outflowDetails?.Select(t => new InflowDetailsDataModel()
                {
                    Id = t.Id,
                    ProductDetail = t.ProductDetail,
                    ProductDetailId = t.ProductDetailId,
                    Quantity = t.Quantity,
                })?.ToList() ?? []
            };

            var res = await inflows.Create(inflow);


            // Retorna
            return new(Responses.Success, res.LastId);

        }
        catch (Exception)
        {
        }

        return new();
    }


    public async Task<ReadOneResponse<int>> GetInventory(int outflow)
    {
        // Ejecución
        try
        {

            var inventory = await (from a in context.Salidas
                                   where a.Id == outflow
                                   select a.InventoryId).FirstOrDefaultAsync();
            // Retorna
            return new(Responses.Success)
            {
                Model = inventory
            };

        }
        catch (Exception)
        {
        }

        return new();
    }

}