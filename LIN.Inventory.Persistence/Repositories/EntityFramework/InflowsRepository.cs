namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class InflowsRepository(Context.Context context, ILogger<InflowsRepository> logger) : IInflowsRepository
{

    /// <summary>
    /// Crea una entrada de inventario.
    /// </summary>
    /// <param name="data">Modelo de la entrada</param>
    public async Task<CreateResponse> Create(InflowDataModel data)
    {

        // Modelo
        data.Id = 0;
        data.OutflowRelatedId = null;
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

                context.Attach(data.Inventory);

                if (data.Profile is not null)
                    context.Attach(data.Profile);

                if (data.OutflowRelated is not null)
                    data.OutflowRelated = context.AttachOrUpdate(data.OutflowRelated);

                // Entrada base
                await context.Entradas.AddAsync(data);

                // Guarda cambios
                context.SaveChanges();

                // Detalles
                foreach (var detail in details)
                {
                    // Modelo details
                    detail.Id = 0;
                    detail.ProductDetail = new()
                    {
                        Id = detail.ProductDetailId
                    };

                    detail.Movement = data;

                    detail.ProductDetail = context.AttachOrUpdate(detail.ProductDetail);

                    // Agregar los detalles.
                    context.DetallesEntradas.Add(detail);

                    // Si la cantidad es invalida
                    if (detail.Quantity <= 0 && data.Type != InflowsTypes.Correction || detail.Quantity < 0 && data.Type == InflowsTypes.Correction)
                        throw new Exception("Invalid detail quantity");

                    // Producto
                    var productDetail = from dt in context.ProductoDetalles
                                        where dt.Id == detail.ProductDetailId
                                        && dt.Status == ProductStatements.Normal
                                        select dt;

                    // Si el movimiento es aceptado, actualizamos el stock.
                    if (data.Status == MovementStatus.Approved)
                    {
                        // Ajustar.
                        if (data.Type == InflowsTypes.Correction)
                            await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => detail.Quantity));

                        // Sumar.
                        else
                            await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity + detail.Quantity));

                    }

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
    /// Obtiene una entrada.
    /// </summary>
    /// <param name="id">Id de la entrada</param>
    public async Task<ReadOneResponse<InflowDataModel>> Read(int id, bool includeDetails)
    {

        // Ejecución
        try
        {

            // Consulta.
            InflowDataModel? inflow = await (from i in context.Entradas
                                             where i.Id == id
                                             select new InflowDataModel
                                             {
                                                 Id = i.Id,
                                                 Date = i.Date,
                                                 Type = i.Type,
                                                 InventoryId = i.InventoryId,
                                                 Profile = i.Profile,
                                                 ProfileId = i.ProfileId,
                                                 OutflowRelatedId = i.OutflowRelatedId,
                                             }).FirstOrDefaultAsync();

            // Validar.
            if (inflow == null)
                return new(Responses.NotRows);

            // Incluir detalles.
            if (includeDetails)
            {

                // Consulta de detalles.
                inflow.Details = await (from de in context.DetallesEntradas
                                        where de.MovementId == id
                                        select new InflowDetailsDataModel
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
                                                    Image = de.ProductDetail.Product.Image
                                                }
                                            }
                                        }).ToListAsync();
            }

            // No incluir detalles.
            else
                inflow.CountDetails = context.DetallesEntradas.Count(t => t.MovementId == id);

            // Calcular inversion.
            inflow.Inversion = await (from de in context.DetallesEntradas
                                      where de.Movement.Type == InflowsTypes.Purchase
                                      where de.MovementId == id
                                      select de.ProductDetail.PurchasePrice * de.Quantity).SumAsync();

            // Calcular inversion.
            inflow.Prevision = await (from de in context.DetallesEntradas
                                      where de.MovementId == id
                                      select (
                                      de.Movement.Type == InflowsTypes.Purchase
                                      ? (de.ProductDetail.SalePrice - de.ProductDetail.PurchasePrice) * de.Quantity
                                      : de.Movement.Type == InflowsTypes.Gift ? de.ProductDetail.SalePrice * de.Quantity : 0)).SumAsync();

            // Retorna
            return new(Responses.Success, inflow);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtiene la lista de entradas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    public async Task<ReadAllResponse<InflowDataModel>> ReadAll(int id)
    {
        // Ejecución
        try
        {

            var res = from E in context.Entradas
                      where E.InventoryId == id
                      orderby E.Date descending
                      select new InflowDataModel()
                      {
                          Id = E.Id,
                          Date = E.Date,
                          InventoryId = E.InventoryId,
                          ProfileId = E.ProfileId,
                          Type = E.Type,
                          CountDetails = context.DetallesEntradas.Count(t => t.MovementId == E.Id),
                          Profile = E.Profile != null ? new()
                          {
                              Id = E.Profile.Id,
                              AccountId = E.Profile.AccountId
                          } : null,
                          OutflowRelatedId = E.OutflowRelatedId
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
    /// Actualizar la fecha de una entrada.
    /// </summary>
    /// <param name="id">Id de la entrada.</param>
    /// <param name="date">Nueva fecha.</param>
    public async Task<ResponseBase> Update(int id, DateTime date)
    {

        // Ejecución
        try
        {

            // Update.
            var update = await (from inflow in context.Entradas
                                where inflow.Id == id
                                select inflow).ExecuteUpdateAsync(t => t.SetProperty(t => t.Date, date));

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



    public async Task<ResponseBase> Comfirm(int id)
    {

        // Ejecución
        try
        {
            // Update.
            var update = await (from inflow in context.Entradas
                                where inflow.Id == id
                                select inflow).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, MovementStatus.Approved));


            var data = await (from inflow in context.Entradas
                              where inflow.Id == id
                              select inflow).FirstOrDefaultAsync();

            // Obtener los detalles.
            var details = await (from detail in context.DetallesEntradas
                                 where detail.MovementId == id
                                 select detail).ToListAsync();

            // Detalles
            foreach (var detail in details)
            {
                // Producto
                var productDetail = from dt in context.ProductoDetalles
                                    where dt.Id == detail.ProductDetailId
                                    && dt.Status == ProductStatements.Normal
                                    select dt;

                // Ajustar.
                if (data.Type == InflowsTypes.Correction)
                    await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => detail.Quantity));

                // Sumar.
                else
                    await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity + detail.Quantity));


            }

            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }
        return new();
    }


    /// <summary>
    /// Informe.
    /// </summary>
    /// <param name="month">Mes</param>
    /// <param name="year">Año</param>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadAllResponse<InflowRow>> Informe(int month, int year, int inventory)
    {

        // Ejecución
        try
        {

            // Selecciona
            var query = from E in context.Entradas
                        where E.InventoryId == inventory
                        && E.Date.Year == year
                        && E.Date.Month == month
                        join ED in context.DetallesEntradas
                        on E.Id equals ED.MovementId
                        join P in context.ProductoDetalles
                        on ED.ProductDetailId equals P.Id
                        select new InflowRow
                        {
                            PrecioCompra = P.PurchasePrice,
                            PrecioVenta = P.SalePrice,
                            Fecha = E.Date,
                            ProductCode = P.Product.Code,
                            ProductName = P.Product.Name,
                            Cantidad = ED.Quantity,
                            Type = E.Type
                        };


            var entradas = await query.ToListAsync();

            // Retorna
            return new(Responses.Success, entradas);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }
        return new();
    }


    /// <summary>
    /// Obtiene la lista de compras asociadas a un usuario
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="days">Dias hacia atrás</param>
    public async Task<ReadOneResponse<int>> ComprasOf(int id, int days)
    {

        // Ejecución
        try
        {

            var actualDate = DateTime.Now;
            var lastDate = actualDate.AddDays(-days);

            // Selecciona la entrada
            var query = from AI in context.AccesoInventarios
                        where AI.ProfileId == id && AI.State == InventoryAccessState.Accepted
                        join I in context.Inventarios on AI.InventoryId equals I.Id
                        join E in context.Entradas on I.Id equals E.InventoryId
                        where E.ProfileId == id && E.Type == InflowsTypes.Purchase && E.Date >= lastDate
                        join SD in context.DetallesSalidas on E.Id equals SD.MovementId
                        select SD;


            var ventas = await query.CountAsync();

            // Retorna
            return new(Responses.Success, ventas);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    public async Task<ReadOneResponse<int>> GetInventory(int inflow)
    {
        // Ejecución
        try
        {

            var inventory = await (from a in context.Entradas
                                   where a.Id == inflow
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