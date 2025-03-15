using LIN.Types.Inventory.Enumerations;
using LIN.Types.Inventory.Transient;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;


namespace LIN.Inventory.Persistence.Data;

public class Statistics(Context.Context context)
{

    /// <summary>
    /// Ventas de un perfil en un periodo de tiempo.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="initDate">Fecha inicial.</param>
    /// <param name="endDate">Fecha final.</param>
    public async Task<ReadOneResponse<decimal>> Sales(int profile, DateTime initDate, DateTime endDate)
    {

        try
        {

            // Consulta.
            var query = from AI in context.AccesoInventarios
                        where AI.ProfileId == profile
                        && AI.State == InventoryAccessState.Accepted
                        join I in context.Inventarios on AI.InventoryId equals I.Id
                        join S in context.Salidas on I.Id equals S.InventoryId
                        where S.ProfileId == profile
                        && S.Type == OutflowsTypes.Purchase
                        && S.Date >= initDate
                        && S.Date <= endDate
                        join SD in context.DetallesSalidas on S.Id equals SD.MovementId
                        join P in context.ProductoDetalles on SD.ProductDetailId equals P.Id
                        orderby S.Date
                        select P.SalePrice * SD.Quantity;


            // Contar.
            decimal total = await query.SumAsync();

            // Retornar.
            return new()
            {
                Response = Responses.Success,
                Model = total
            };

        }
        catch (Exception)
        {
        }

        return new();
    }


    /// <summary>
    /// Ventas de un perfil en un periodo de tiempo.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="initDate">Fecha inicial.</param>
    /// <param name="endDate">Fecha final.</param>
    public async Task<ReadAllResponse<SalesModel>> SalesOn(int profile, DateTime initDate, DateTime endDate)
    {

        try
        {

            // Selecciona la entrada
            var query = await (from AI in context.AccesoInventarios
                               where AI.ProfileId == profile
                               && AI.State == InventoryAccessState.Accepted
                               join I in context.Inventarios on AI.InventoryId equals I.Id
                               join S in context.Salidas on I.Id equals S.InventoryId
                               where S.ProfileId == profile
                               && S.Type == OutflowsTypes.Purchase
                               && S.Date >= initDate
                               && S.Date <= endDate
                               join SD in context.DetallesSalidas on S.Id equals SD.MovementId
                               join P in context.ProductoDetalles on SD.ProductDetailId equals P.Id
                               orderby S.Date
                               select new SalesModel
                               {
                                   Money = P.SalePrice * SD.Quantity,
                                   Date = S.Date
                               }).ToListAsync();

            // Retorna
            return new(Responses.Success, query);

        }
        catch (Exception)
        {
        }

        return new();
    }

}