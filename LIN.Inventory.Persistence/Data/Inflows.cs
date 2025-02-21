using LIN.Types.Inventory.Enumerations;
using LIN.Types.Inventory.Models;
using LIN.Types.Inventory.Transient;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIN.Inventory.Persistence.Data;

public class Inflows(Context.Context context, ILogger<Inflows> logger)
{

    /// <summary>
    /// Crea una entrada de inventario.
    /// </summary>
    /// <param name="data">Modelo de la entrada</param>
    public async Task<CreateResponse> Create(InflowDataModel data)
    {

        // Modelo
        data.Id = 0;
        data.Profile = new() { Id = data.ProfileID };

        // Ejecución (Transacción)
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {

                // Detalles.
                var details = data.Details;
                data.Details = [];

                context.Attach(data.Inventory);
                context.Attach(data.Profile);

                // Entrada base
                await context.Entradas.AddAsync(data);

                // Guarda cambios
                context.SaveChanges();

                // Detalles
                foreach (var detail in details)
                {
                    // Modelo details
                    detail.Id = 0;
                    detail.Movement = data;

                    // Agregar los detalles.
                    context.DetallesEntradas.Add(detail);

                    context.Attach(detail.ProductDetail);

                    // Si la cantidad es invalida
                    if (detail.Cantidad <= 0 && data.Type != InflowsTypes.Ajuste || detail.Cantidad < 0 && data.Type == InflowsTypes.Ajuste)
                        throw new Exception("Invalid detail quantity");

                    // Producto
                    var productDetail = from dt in context.ProductoDetalles
                                        where dt.Id == detail.ProductDetailId
                                        && dt.Estado == ProductStatements.Normal
                                        select dt;

                    // Ajustar.
                    if (data.Type == InflowsTypes.Ajuste)
                        await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => detail.Cantidad));

                    // Sumar.
                    else
                        await productDetail.ExecuteUpdateAsync(s => s.SetProperty(e => e.Quantity, e => e.Quantity + detail.Cantidad));


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
                                                 ProfileID = i.ProfileID
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
                inflow.CountDetails = context.DetallesEntradas.Count(t => t.MovementId == id);

            // Calcular inversion.
            inflow.Inversion = await (from de in context.DetallesEntradas
                                      where de.Movement.Type == InflowsTypes.Compra
                                      where de.MovementId == id
                                      select de.ProductDetail.PrecioCompra * de.Cantidad).SumAsync();

            // Calcular inversion.
            inflow.Prevision = await (from de in context.DetallesEntradas
                                      where de.MovementId == id
                                      select (
                                      de.Movement.Type == InflowsTypes.Compra
                                      ? (de.ProductDetail.PrecioVenta - de.ProductDetail.PrecioCompra) * de.Cantidad
                                      : de.Movement.Type == InflowsTypes.Regalo ? de.ProductDetail.PrecioVenta * de.Cantidad : 0)).SumAsync();

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
                          ProfileID = E.ProfileID,
                          Type = E.Type,
                          CountDetails = context.DetallesEntradas.Count(t => t.MovementId == E.Id)
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
                        join I in context.Inventarios on AI.Inventario equals I.Id
                        join E in context.Entradas on I.Id equals E.InventoryId
                        where E.ProfileID == id && E.Type == InflowsTypes.Compra && E.Date >= lastDate
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


}