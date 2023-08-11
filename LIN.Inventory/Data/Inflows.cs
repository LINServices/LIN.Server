namespace LIN.Server.Data;

public class Inflows
{


    #region Abstracciones


    /// <summary>
    /// Crea una entrada de inventario
    /// </summary>
    /// <param name="data">Modelo de la entrada</param>
    public async static Task<CreateResponse> Create(InflowDataModel data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();


        var response = await Create(data, context);
        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene una entrada
    /// </summary>
    /// <param name="id">ID de la entrada</param>
    /// <param name="mask">Si es una mascara</param>
    public async static Task<ReadOneResponse<InflowDataModel>> Read(int id, bool mask = false)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Read(id, mask, context);
        context.CloseActions(connectionKey);
        return response;
    }



    /// <summary>
    /// Obtiene la lista de entradas asociadas a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    public async static Task<ReadAllResponse<InflowDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return response;
    }



    #endregion



    /// <summary>
    /// Crea una entrada de inventario
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
                // Entrada base
                await context.DataBase.Entradas.AddAsync(data);

                // Guarda cambios
                context.DataBase.SaveChanges();

                // Detalles
                foreach (var detail in data.Details)
                {
                    // Modelo details
                    detail.ID = 0;
                    detail.Movimiento = data.ID;
                    context.DataBase.DetallesEntradas.Add(detail);

                    // Si la cantidad es invalida
                    if (detail.Cantidad <= 0 && data.Type != InflowsTypes.Ajuste || detail.Cantidad < 0 && data.Type == InflowsTypes.Ajuste)
                        throw new Exception("Invalid detail quantity");

                    // Producto
                    var productodetail = context.DataBase.ProductoDetalles.Where(T => T.ID == detail.ProductoDetail && T.Estado == ProductStatements.Normal).FirstOrDefault();

                    // Si el producto no existe
                    if (productodetail == null || productodetail.Estado == ProductStatements.Undefined)
                        throw new Exception("No existe el producto");


                    if (data.Type == InflowsTypes.Ajuste)
                        productodetail.Quantity = detail.Cantidad;
                    else
                        productodetail.Quantity += detail.Cantidad;


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
    /// Obtiene una entrada
    /// </summary>
    /// <param name="id">ID de la entrada</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<InflowDataModel>> Read(int id, bool mask, Conexión context)
    {

        System.Diagnostics.Stopwatch reloj = new();
        reloj.Start();

        // Ejecución
        try
        {
            // Selecciona la entrada
            var entrada = context.DataBase.Entradas.FirstOrDefault(T => T.ID == id);

            if (entrada == null)
            {
                return new(Responses.NotRows);
            }

            // Si es una mascara
            if (mask)
                entrada.CountDetails = context.DataBase.DetallesEntradas.Count(t => t.Movimiento == id);

            // Si se necesitan los detales
            else
            {
                entrada.Details = await context.DataBase.DetallesEntradas.Where(T => T.Movimiento == id).ToListAsync();

                // Calcula la inversión
                var allInversions = from DE in context.DataBase.DetallesEntradas
                                    where DE.Movimiento == id
                                    join PD in context.DataBase.ProductoDetalles
                                    on DE.ProductoDetail equals PD.ID
                                    select PD.PrecioCompra * DE.Cantidad;

                var inversion = allInversions.Sum();
                entrada.Inversion = inversion;

            }


            reloj.Stop();
            ServerLogger.LogError("Time ReadOne: " + reloj.ElapsedMilliseconds);

            // Retorna
            return new(Responses.Success, entrada);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de entradas asociadas a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<InflowDataModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = from E in context.DataBase.Entradas
                      where E.Inventario == id
                      orderby E.Date ascending
                      select new InflowDataModel()
                      {
                          ID = E.ID,
                          Date = E.Date,
                          Inventario = E.Inventario,
                          Usuario = E.Usuario,
                          Type = E.Type,
                          CountDetails = context.DataBase.DetallesEntradas.Count(t => t.Movimiento == E.ID)
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





    public async static Task<ReadAllResponse<InflowRow>> Informe(int month, int year, int inventory, Conexión context)
    {

        // Ejecución
        try
        {

            // Selecciona
            var query = from E in context.DataBase.Entradas
                        where E.Inventario == inventory
                        && E.Date.Year == year && E.Date.Month == month
                        join ED in context.DataBase.DetallesEntradas
                        on E.ID equals ED.Movimiento
                        join P in context.DataBase.ProductoDetalles
                        on ED.ProductoDetail equals P.ID
                        join PR in context.DataBase.PlantillaProductos
                        on P.ProductoFK equals PR.ID
                        select new InflowRow
                        {
                            PrecioCompra = P.PrecioCompra,
                            PrecioVenta = P.PrecioVenta,
                            Fecha = E.Date,
                            ProductCode = PR.Code,
                            ProductName = PR.Name,
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
    /// <param name="id">ID del usuario</param>
    /// <param name="days">Dias hacia atrás</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> ComprasOf(int id, int days, Conexión context)
    {

        System.Diagnostics.Stopwatch reloj = new();
        reloj.Start();


        // Ejecución
        try
        {

            var actualDate = DateTime.Now;
            var lastDate = actualDate.AddDays(-days);

            // Selecciona la entrada
            var query = from AI in context.DataBase.AccesoInventarios
                        where AI.Usuario == id && AI.State == InventoryAccessState.Accepted
                        join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                        join E in context.DataBase.Entradas on I.ID equals E.Inventario
                        where E.Usuario == id && E.Type == InflowsTypes.Compra && E.Date >= lastDate
                        join SD in context.DataBase.DetallesSalidas on E.ID equals SD.Movimiento
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