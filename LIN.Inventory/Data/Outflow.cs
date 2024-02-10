﻿using LIN.Types.Inventory.Transient;

namespace LIN.Inventory.Data;


public class Outflows
{



    #region Abstracciones


    /// <summary>
    /// Crea una salida de inventario
    /// </summary>
    /// <param name="data">Modelo de la salida</param>
    public async static Task<CreateResponse> Create(OutflowDataModel data)
    {
        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene una salida
    /// </summary>
    /// <param name="id">Id de la salida</param>
    public async static Task<ReadOneResponse<OutflowDataModel>> Read(int id, bool mask = false)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(id, mask, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene la lista de salidas asociadas a un inventario
    /// </summary>
    /// <param name="id">Id del inventario</param>
    public async static Task<ReadAllResponse<OutflowDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene las ventas realizadas por un usuario en todos los inventarios
    /// </summary>
    public async static Task<ReadOneResponse<int>> VentasOf(int id, int days)
    {

        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await VentasOf(id, days, context);
        context.CloseActions(connectionKey);
        return res;

    }




    public async static Task<ReadAllResponse<SalesModel>> Ventas(int id, int days)
    {

        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await Ventas(id, days, context);
        context.CloseActions(connectionKey);
        return res;

    }



    #endregion



    /// <summary>
    /// Crea una salida de inventario
    /// </summary>
    /// <param name="data">Modelo de la salida</param>
    /// <param name="context">Contexto de conexión</param>
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

                    if (detail.Cantidad <= 0)
                        throw new Exception("Invalid detail quantity");

                    // Detalle de un producto
                    var productoDetail = context.DataBase.ProductoDetalles.Where(T => T.Id == detail.ProductDetailId && T.Estado == ProductStatements.Normal).FirstOrDefault();

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
                        // Obtiene el producto
                        var producto = context.DataBase.Productos.Where(T => T.Id == productoDetail.ProductId).FirstOrDefault();

                        // Si no existe un producto
                        if (producto == null)
                            return new(Responses.NotRows, -1, "No existe un producto");

                        return new(Responses.InvalidParam, -1, $"El producto no tiene stock suficiente");

                    }


                    // Disminuye la cantidad
                    productoDetail.Quantity -= detail.Cantidad;

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
    /// Obtiene una salida
    /// </summary>
    /// <param name="id">Id de la salida</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<OutflowDataModel>> Read(int id, bool mask, Conexión context)
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
            if (mask)
                salida.CountDetails = context.DataBase.DetallesSalidas.Count(t => t.MovementId == id);

            // Si se necesitan los detales
            else
                salida.Details = await context.DataBase.DetallesSalidas.Where(T => T.MovementId == id).ToListAsync();

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
    /// Obtiene la lista de salidas asociadas a un inventario
    /// </summary>
    /// <param name="id">Id del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<OutflowDataModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = from S in context.DataBase.Salidas
                      where S.InventoryId == id
                      orderby S.Date ascending
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
    /// Obtiene las ventas realizadas por un usuario en todos los inventarios
    /// </summary>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> VentasOf(int id, int days, Conexión context)
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
                        join S in context.DataBase.Salidas on I.ID equals S.InventoryId
                        where S.ProfileID == id && S.Type == OutflowsTypes.Venta && S.Date >= lastDate
                        join SD in context.DataBase.DetallesSalidas on S.ID equals SD.MovementId
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



    public async static Task<ReadAllResponse<OutflowRow>> Informe(int month, int year, int inventory, Conexión context)
    {

        // Ejecución
        try
        {

            // Selecciona
            var query = from E in context.DataBase.Salidas
                        where E.InventoryId == inventory
                        && E.Date.Year == year && E.Date.Month == month
                        join ED in context.DataBase.DetallesSalidas
                        on E.ID equals ED.MovementId
                        join P in context.DataBase.ProductoDetalles
                        on ED.ProductDetailId equals P.Id
                        select new OutflowRow
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
    /// Obtiene las ventas realizadas por un usuario en todos los inventarios
    /// </summary>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<SalesModel>> Ventas(int profile, int days, Conexión context)
    {

        // Ejecución
        try
        {

            var actualDate = DateTime.Now;
            var lastDate = actualDate.AddDays(-days);




            // Selecciona la entrada
            var query = from AI in context.DataBase.AccesoInventarios
                        where AI.ProfileID == profile && AI.State == InventoryAccessState.Accepted
                        join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                        join S in context.DataBase.Salidas on I.ID equals S.InventoryId
                        where S.ProfileID == profile && S.Type == OutflowsTypes.Venta && S.Date >= lastDate
                        join SD in context.DataBase.DetallesSalidas on S.ID equals SD.MovementId
                        join P in context.DataBase.ProductoDetalles on SD.ProductDetailId equals P.Id
                        orderby S.Date
                        select new SalesModel{
                            Money = P.PrecioVenta * SD.Cantidad,
                            Date = S.Date
                                };


          
            // Retorna
            return new(Responses.Success, query.ToList());

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);


        }

        return new();
    }


}