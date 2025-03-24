namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class HoldsRepository(Context.Context context) : IHoldsRepository
{

    /// <summary>
    /// Crear reserva de un producto.
    /// </summary>
    /// <param name="model">Modelo.</param>
    /// <param name="contextTransaction">Transacción actual.</param>
    public async Task<CreateResponse> Create(HoldModel model, IDbContextTransaction? contextTransaction = null)
    {

        // Validar modelo.
        model.DetailModel = new ProductDetailModel() { Id = model.DetailId };

        var transaction = contextTransaction ?? context.Database.BeginTransaction();
        try
        {
            // Existe el detalle.
            model.DetailModel = context.AttachOrUpdate(model.DetailModel);

            // Guardar el modelo.
            context.Holds.Add(model);
            await context.SaveChangesAsync();

            // Actualizar el stock del inventario.
            var product = await (from p in context.ProductoDetalles
                                 where p.Id == model.DetailId
                                 select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Quantity, a => a.Quantity - model.Quantity));

            if (contextTransaction is null)
                transaction.Commit();

            return new(Responses.Success, model.Id);
        }
        catch (Exception)
        {
            transaction.Rollback();
        }

        return new();
    }


    /// <summary>
    /// Retornar al stock de inventario un hold.
    /// </summary>
    /// <param name="holdId">Hold Id.</param>
    public async Task<CreateResponse> Return(int holdId)
    {
        try
        {

            // Obtener la reserva.
            var hold = await (from p in context.Holds
                              where p.Id == holdId
                              && p.Status == HoldStatus.None
                              select p).FirstOrDefaultAsync();

            // Si no se encontró.
            if (hold is null)
                return new(Responses.NotRows);

            // Actualizar el producto.
            var product = await (from p in context.ProductoDetalles
                                 where p.Id == hold.DetailId
                                 select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Quantity, a => a.Quantity + hold.Quantity));

            // Actualizar el estado del hold.
            var holds = await (from p in context.Holds
                               where p.Id == hold.Id
                               select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, HoldStatus.Reversed));

            return new(Responses.Success);
        }
        catch (Exception)
        {

        }
        return new();
    }


    /// <summary>
    /// Aprobar el estado del hold.
    /// </summary>
    /// <param name="holdId">Id del hold.</param>
    public async Task<CreateResponse> Approve(int holdId)
    {
        try
        {

            // Obtener el hold.
            var hold = await (from p in context.Holds
                              where p.Id == holdId
                              && p.Status == HoldStatus.None
                              select p).FirstOrDefaultAsync();

            // Si no se encontró.
            if (hold is null)
                return new(Responses.NotRows);

            // Actualizar el estado.
            var holds = await (from p in context.Holds
                               where p.Id == hold.Id
                               select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, HoldStatus.Approve));

            return new(Responses.Success);
        }
        catch (Exception)
        {
        }

        return new();
    }

}