namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class HoldsGroupRepository(Context.Context context, IHoldsRepository holdsRepository) : IHoldsGroupRepository
{

    /// <summary>
    /// Crear nuevo grupo de reservas.
    /// </summary>
    /// <param name="model">Modelo.</param>
    public async Task<CreateResponse> Create(HoldGroupModel model)
    {

        // Obtener la transacción.
        using var transaction = context.Database.BeginTransaction();
        try
        {

            // Holds.
            var data = model.Holds;
            model.Holds = [];

            // Guardar el registro principal.
            context.HoldGroups.Add(model);
            context.SaveChanges();

            // Guardar cada reserva.
            foreach (var hold in data)
            {
                hold.GroupModel = model;
                hold.GroupId = model.Id;
                var result = await holdsRepository.Create(hold, transaction);
            }

            // Guardar todos los cambios.
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
    /// Retornar al inventarios las reservas asociadas a un grupo de reserva.
    /// </summary>
    /// <param name="holdGroupId">Id del grupo de reserva.</param>
    public async Task<CreateResponse> Return(int holdGroupId)
    {
        try
        {
            // Obtener las reservas.
            var holds = await (from p in context.Holds
                               where p.GroupId == holdGroupId
                               select p.Id).ToListAsync();

            foreach (var holdId in holds)
                await holdsRepository.Return(holdId);

            return new(Responses.Success);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Aprobar el estado de un holds group.
    /// </summary>
    /// <param name="holdGroupId">Id del holds group.</param>
    public async Task<CreateResponse> Approve(int holdGroupId)
    {
        try
        {
            // Obtener holds.
            var holds = await (from p in context.Holds
                               where p.GroupId == holdGroupId
                               && p.Status == HoldStatus.None
                               select p.Id).ToListAsync();

            // Aprobar.
            foreach (var holdId in holds)
                await holdsRepository.Approve(holdId);

            return new(Responses.Success);
        }
        catch (Exception)
        {

        }
        return new();
    }


    /// <summary>
    /// Obtener las reservas asociadas a un grupo.
    /// </summary>
    /// <param name="holdGroupId">Id del grupo.</param>
    public async Task<ReadAllResponse<HoldModel>> GetItems(int holdGroupId)
    {
        try
        {
            var holds = await (from p in context.Holds
                               where p.GroupId == holdGroupId
                               select new HoldModel
                               {
                                   DetailModel = new()
                                   {
                                       Id = p.DetailModel.Id,
                                       SalePrice = p.DetailModel.SalePrice,
                                       Product = new()
                                       {
                                           Id = p.DetailModel.Product.Id,
                                           Name = p.DetailModel.Product.Name
                                       }
                                   },
                                   Status = p.Status,
                                   Quantity = p.Quantity,
                                   Id = p.Id
                               }).ToListAsync();

            return new(Responses.Success, holds);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtener el inventario al que se asocia un grupo.
    /// </summary>
    /// <param name="holdGroupId">Id del grupo.</param>
    public async Task<ReadOneResponse<int>> GetInventory(int holdGroupId)
    {
        try
        {

            var hold = await (from p in context.Holds
                              where p.GroupId == holdGroupId
                              select p.DetailModel.Product.InventoryId).FirstOrDefaultAsync();

            if (hold <= 0)
                return new(Responses.NotRows);

            return new(Responses.Success, hold);
        }
        catch (Exception)
        {

        }
        return new();
    }

    /// <summary>
    /// Obtener el inventario al que se asocia un grupo.
    /// </summary>
    /// <param name="holdGroupId">Id del grupo.</param>
    public async Task<ReadOneResponse<HoldGroupModel>> Read(int holdGroupId)
    {
        try
        {

            var hold = await (from p in context.HoldGroups
                              where p.Id == holdGroupId
                              select p).FirstOrDefaultAsync();

            if (hold is null)
                return new(Responses.NotRows);

            return new(Responses.Success, hold);
        }
        catch (Exception)
        {

        }
        return new();
    }


    /// <summary>
    /// Obtener los grupos de reservas asociados a un inventario (holds sin estado)
    /// </summary>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadAllResponse<HoldModel>> GetItemsHolds(int inventory)
    {
        try
        {
            var holds = await (from p in context.Holds
                               where p.DetailModel.Product.InventoryId == inventory
                               && p.Status == HoldStatus.None
                               select new HoldModel
                               {
                                   DetailModel = new()
                                   {
                                       Id = p.DetailModel.Id,
                                       SalePrice = p.DetailModel.SalePrice,
                                       Product = new()
                                       {
                                           Id = p.DetailModel.Product.Id,
                                           Name = p.DetailModel.Product.Name
                                       }
                                   },
                                   Status = p.Status,
                                   Quantity = p.Quantity,
                                   GroupId = p.GroupId,
                                   Id = p.Id
                               }).ToListAsync();

            return new(Responses.Success, holds);
        }
        catch (Exception)
        {
        }
        return new();
    }

}