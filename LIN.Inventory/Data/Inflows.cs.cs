namespace LIN.Inventory.Data;


public partial class Inflows
{


    /// <summary>
    /// Crea una entrada de inventario.
    /// </summary>
    /// <param name="data">Modelo de la entrada</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(InflowDataModel data, Conexión context)
    {

        // Modelo
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
                await context.DataBase.Entradas.AddAsync(data);

                // Guarda cambios
                context.DataBase.SaveChanges();

                // Detalles
                foreach (var detail in details)
                {
                    // Modelo details
                    detail.ID = 0;
                    detail.Movement = data;

                    // Agregar los detalles.
                    context.DataBase.DetallesEntradas.Add(detail);

                    context.DataBase.Attach(detail.ProductDetail);

                    // Si la cantidad es invalida
                    if (detail.Cantidad <= 0 && data.Type != InflowsTypes.Ajuste || detail.Cantidad < 0 && data.Type == InflowsTypes.Ajuste)
                        throw new Exception("Invalid detail quantity");

                    // Producto
                    var productDetail = (from dt in context.DataBase.ProductoDetalles
                                         where dt.Id == detail.ProductDetailId
                                         && dt.Estado == ProductStatements.Normal
                                         select dt);


                    // Ajustar.
                    if (data.Type == InflowsTypes.Ajuste)
                        await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => detail.Cantidad));

                    // Sumar.
                    else
                        await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity + detail.Cantidad));


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
    /// Obtiene una entrada.
    /// </summary>
    /// <param name="id">Id de la entrada</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<InflowDataModel>> Read(int id, bool includeDetails, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta.
            InflowDataModel? inflow = await (from i in context.DataBase.Entradas
                                             where i.ID == id
                                             select i).FirstOrDefaultAsync();

            // Validar.
            if (inflow == null)
                return new(Responses.NotRows);


            // Incluir detalles.
            if (includeDetails)
            {

                // Consulta de detalles.
                inflow.Details = await (from de in context.DataBase.DetallesEntradas
                                        where de.MovementId == id
                                        select new InflowDetailsDataModel
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

            // No incluir detalles.
            else
                inflow.CountDetails = context.DataBase.DetallesEntradas.Count(t => t.MovementId == id);

            // Calcular inversion.
            inflow.Inversion = await (from de in context.DataBase.DetallesEntradas
                                      where de.Movement.Type == InflowsTypes.Compra
                                      where de.MovementId == id
                                      select de.ProductDetail.PrecioCompra * de.Cantidad).SumAsync();

            // Calcular inversion.
            inflow.Prevision = await (from de in context.DataBase.DetallesEntradas
                                      where de.MovementId == id
                                      select (
                                      de.Movement.Type == InflowsTypes.Compra
                                      ? (de.ProductDetail.PrecioVenta - de.ProductDetail.PrecioCompra) * de.Cantidad
                                      : ( de.Movement.Type == InflowsTypes.Regalo ? de.ProductDetail.PrecioVenta * de.Cantidad : 0 ) )).SumAsync();

            // Retorna
            return new(Responses.Success, inflow);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de entradas asociadas a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<InflowDataModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = from E in context.DataBase.Entradas
                      where E.InventoryId == id
                      orderby E.Date descending
                      select new InflowDataModel()
                      {
                          ID = E.ID,
                          Date = E.Date,
                          InventoryId = E.InventoryId,
                          ProfileID = E.ProfileID,
                          Type = E.Type,
                          CountDetails = context.DataBase.DetallesEntradas.Count(t => t.MovementId == E.ID)
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
    /// Actualizar la fecha de una entrada.
    /// </summary>
    /// <param name="id">Id de la entrada.</param>
    /// <param name="date">Nueva fecha.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> Update(int id, DateTime date, Conexión context)
    {

        // Ejecución
        try
        {

            // Update.
            var update = await (from inflow in context.DataBase.Entradas
                                where inflow.ID == id
                                select inflow).ExecuteUpdateAsync(t => t.SetProperty(t => t.Date, date));

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
























    public async static Task<ReadAllResponse<InflowRow>> Informe(int month, int year, int inventory, Conexión context)
    {

        // Ejecución
        try
        {

            // Selecciona
            var query = from E in context.DataBase.Entradas
                        where E.InventoryId == inventory
                        && E.Date.Year == year && E.Date.Month == month
                        join ED in context.DataBase.DetallesEntradas
                        on E.ID equals ED.MovementId
                        join P in context.DataBase.ProductoDetalles
                        on ED.ProductDetailId equals P.Id
                        select new InflowRow
                        {
                            PrecioCompra = P.PrecioCompra,
                            PrecioVenta = P.PrecioVenta,
                            Fecha = E.Date,
                            ProductCode = P.Product.Code,
                            ProductName = P.Product.Name,
                            Cantidad = ED.Cantidad,
                            Type = E.Type
                        };


            var entradas = await query.ToListAsync();

            // Retorna
            return new(Responses.Success, entradas);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }


        return new();
    }














    /// <summary>
    /// Obtiene la lista de compras asociadas a un usuario
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="days">Dias hacia atrás</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> ComprasOf(int id, int days, Conexión context)
    {

        Stopwatch reloj = new();
        reloj.Start();


        // Ejecución
        try
        {

            var actualDate = DateTime.Now;
            var lastDate = actualDate.AddDays(-days);

            // Selecciona la entrada
            var query = from AI in context.DataBase.AccesoInventarios
                        where AI.ProfileID == id && AI.State == InventoryAccessState.Accepted
                        join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                        join E in context.DataBase.Entradas on I.ID equals E.InventoryId
                        where E.ProfileID == id && E.Type == InflowsTypes.Compra && E.Date >= lastDate
                        join SD in context.DataBase.DetallesSalidas on E.ID equals SD.MovementId
                        select SD;


            var ventas = await query.CountAsync();

            reloj.Stop();
            ServerLogger.LogError("Time: " + reloj.ElapsedMilliseconds);

            // Retorna
            return new(Responses.Success, ventas);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);


        }

        return new();
    }

}