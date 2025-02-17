using LIN.Types.Inventory.Enumerations;
using LIN.Types.Inventory.Models;
using LIN.Types.Inventory.Transient;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIN.Inventory.Persistence.Data;

public class Outflows(Context.Context context, ILogger<Outflows> logger)
{

    /// <summary>
    /// Crea una salida de inventario.
    /// </summary>
    /// <param name="data">Modelo de la salida.</param>
    public async Task<CreateResponse> Create(OutflowDataModel data)
    {

        data.ID = 0;

        // Ejecución (Transacción)
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                // Detalles.
                var details = data.Details;
                data.Details = [];

                context.Attach(data.Inventory);

                // Entrada base
                await context.Salidas.AddAsync(data);



                // Guarda cambios
                context.SaveChanges();

                // Detalles
                foreach (var detail in details)
                {
                    // Modelo
                    detail.ID = 0;
                    detail.Movement = data;

                    // Agregar los detalles.
                    context.DetallesSalidas.Add(detail);

                    context.Attach(detail.ProductDetail);

                    if (detail.Cantidad <= 0)
                        throw new Exception("Invalid detail quantity");


                    // Detalle de un producto
                    var productoDetail = context.ProductoDetalles.Where(T => T.Id == detail.ProductDetailId && T.Estado == ProductStatements.Normal).Select(t => new { t.Quantity }).FirstOrDefault();

                    // Si no existe el detalle
                    if (productoDetail == null)
                    {
                        logger.LogWarning("No existe el detalle {detalle}", detail.ID);
                        throw new Exception();
                    }


                    // Nuevos datos
                    int newStock = productoDetail.Quantity - detail.Cantidad;


                    // Si el producto no tiene suficiente stock
                    if (newStock < 0)
                    {
                        return new(Responses.InvalidParam, -1, $"El producto no tiene stock suficiente");
                    }


                    await context.ProductoDetalles.Where(t => t.Id == detail.ProductDetailId).ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity - detail.Cantidad));


                }


                // Guarda los detalles
                await context.SaveChangesAsync();

                // Finaliza
                transaction.Commit();
                return new(Responses.Success, data.ID);
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
            var salida = context.Salidas.FirstOrDefault(T => T.ID == id);

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
                salida.CountDetails = context.DetallesSalidas.Count(t => t.MovementId == id);


            // Calcular inversion.
            salida.Inversion = await (from de in context.DetallesSalidas
                                      where de.MovementId == id
                                      select de.ProductDetail.PrecioCompra * de.Cantidad).SumAsync();

            // Calcular ganancia / perdida.
            salida.Ganancia = await (from de in context.DetallesSalidas
                                     where de.MovementId == id
                                     select de.Movement.Type == OutflowsTypes.Venta
                                     ? de.ProductDetail.PrecioVenta * de.Cantidad
                                     : -de.ProductDetail.PrecioCompra * de.Cantidad).SumAsync();

            // Calcular utilidad.
            salida.Utilidad = await (from de in context.DetallesSalidas
                                     where de.Movement.Type == OutflowsTypes.Venta
                                     where de.MovementId == id
                                     select (de.ProductDetail.PrecioVenta - de.ProductDetail.PrecioCompra) * de.Cantidad).SumAsync();

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
                          ID = S.ID,
                          Date = S.Date,
                          InventoryId = S.InventoryId,
                          ProfileID = S.ProfileID,
                          Type = S.Type,
                          CountDetails = context.DetallesSalidas.Count(t => t.MovementId == S.ID)
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
                                where outflow.ID == id
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
                        on E.ID equals ED.MovementId

                        join P in context.ProductoDetalles
                        on ED.ProductDetailId equals P.Id
                        select new OutflowRow
                        {
                            ProductId = P.ProductId,
                            PrecioCompra = P.PrecioCompra,
                            PrecioVenta = P.PrecioVenta,
                            Fecha = E.Date,
                            ProductCode = P.Product.Code,
                            ProductName = P.Product.Name,
                            Cantidad = ED.Cantidad,
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


}