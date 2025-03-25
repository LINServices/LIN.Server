namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class OrdersRepository(Context.Context context) : IOrdersRepository
{

    /// <summary>
    /// Crear nueva orden.
    /// </summary>
    /// <param name="model">Modelo.</param>
    public async Task<CreateResponse> Create(OrderModel model)
    {
        try
        {

            // Si se incluyo un grupo de Holds.
            if (model.HoldGroupId > 0)
            {
                model.HoldGroup = new() { Id = model.HoldGroupId };
                model.HoldGroup = context.AttachOrUpdate(model.HoldGroup);
            }

            // Guardar modelo.
            context.Orders.Add(model);
            await context.SaveChangesAsync();

            return new(Responses.Success, model.Id);
        }
        catch (Exception)
        {
        }

        return new();

    }


    /// <summary>
    /// Actualizar el estado de una orden.
    /// </summary>
    /// <param name="id">Id de la orden.</param>
    /// <param name="status">Nuevo estado.</param>
    public async Task<ResponseBase> Update(int id, string status)
    {
        try
        {

            // Actualizar elementos.
            var itemsUpdated = await (from o in context.Orders
                                      where o.Id == id
                                      select o).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, status));

            return new(Responses.Success);
        }
        catch (Exception)
        {
        }

        return new();

    }


    /// <summary>
    /// Obtener las ordenes asociadas a un id externo.
    /// </summary>
    /// <param name="externalId">Id externo.</param>
    public async Task<ReadAllResponse<OrderModel>> ReadAll(string externalId)
    {

        try
        {
            var orders = await (from o in context.Orders
                                where o.ExternalId == externalId
                                select o).ToListAsync();

            return new(Responses.Success, orders);
        }
        catch (Exception)
        {
        }

        return new();
    }



    public async Task<ReadOneResponse<bool>> HasMovements(int order)
    {
        try
        {
            var orders = await (from o in context.Salidas
                                where o.OrderId == order
                                select o).AnyAsync();

            return new(Responses.Success, orders);
        }
        catch (Exception)
        {
        }

        return new();
    }

}