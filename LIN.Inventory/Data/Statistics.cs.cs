namespace LIN.Inventory.Data;

public partial class Statistics
{



    /// <summary>
    /// Ventas de un perfil en un periodo de tiempo.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="initDate">Fecha inicial.</param>
    /// <param name="endDate">Fecha final.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<int>> Sales(int profile, DateTime initDate, DateTime endDate, Conexión context)
    {

        try
        {

            // Consulta.
            var query = from AI in context.DataBase.AccesoInventarios
                        where AI.ProfileID == profile && AI.State == InventoryAccessState.Accepted
                        join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                        join S in context.DataBase.Salidas on I.ID equals S.InventoryId
                        where S.ProfileID == profile && S.Type == OutflowsTypes.Venta
                        && S.Date >= initDate && S.Date <= endDate
                        join SD in context.DataBase.DetallesSalidas on S.ID equals SD.MovementId
                        select SD;


            // Contar.
            int total = await query.CountAsync();

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
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<SalesModel>> SalesOn(int profile, DateTime initDate, DateTime endDate, Conexión context)
    {

        try
        {

            // Selecciona la entrada
            var query = await (from AI in context.DataBase.AccesoInventarios
                               where AI.ProfileID == profile
                               && AI.State == InventoryAccessState.Accepted
                               join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                               join S in context.DataBase.Salidas on I.ID equals S.InventoryId
                               where S.ProfileID == profile
                               && S.Type == OutflowsTypes.Venta
                               && S.Date >= initDate
                               && S.Date <= endDate
                               join SD in context.DataBase.DetallesSalidas on S.ID equals SD.MovementId
                               join P in context.DataBase.ProductoDetalles on SD.ProductDetailId equals P.Id
                               orderby S.Date
                               select new SalesModel
                               {
                                   Money = P.PrecioVenta * SD.Cantidad,
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