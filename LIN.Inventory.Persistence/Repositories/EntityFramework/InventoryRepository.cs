namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class InventoryRepository(Context.Context context, ILogger<InventoryRepository> logger) : IInventoriesRepository
{

    /// <summary>
    /// Crea un nuevo inventario.
    /// </summary>
    /// <param name="data">Modelo del inventario</param>
    public async Task<CreateResponse> Create(InventoryDataModel data)
    {

        // Modelo
        data.Id = 0;

        // Transacción
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {

                // InventoryId
                context.Inventarios.Add((InventoryDataModel)data);

                // Guarda el inventario
                await context.SaveChangesAsync();

                // Accesos
                DateTime dateTime = DateTime.Now;
                foreach (var acceso in data.UsersAccess)
                {
                    // Propiedades
                    acceso.Id = 0;
                    acceso.Date = dateTime;
                    acceso.InventoryId = data.Id;

                    // Accesos
                    context.AccesoInventarios.Add(acceso);

                }

                // Guarda los cambios
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
    /// Obtiene un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    public async Task<ReadOneResponse<InventoryDataModel>> Read(int id)
    {

        // Ejecución
        try
        {

            var res = await (from i in context.Inventarios
                             where i.Id == id
                             select new InventoryDataModel
                             {
                                 Id = i.Id,
                                 CreatorId = i.CreatorId,
                                 OpenStoreSettingsId = i.OpenStoreSettingsId,
                                 Direction = i.Direction,
                                 Name = i.Name
                             }).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistAccount);

            if (res.OpenStoreSettings != null)
                res.OpenStoreSettings.InventoryDataModel = null!;

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtiene la lista de inventarios asociados a un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async Task<ReadAllResponse<InventoryDataModel>> ReadAll(int id)
    {

        // Ejecución
        try
        {

            var res = from AI in context.AccesoInventarios
                      where AI.ProfileId == id && AI.State == InventoryAccessState.Accepted
                      join I in context.Inventarios on AI.InventoryId equals I.Id
                      select new InventoryDataModel()
                      {
                          CreatorId = I.CreatorId,
                          Direction = I.Direction,
                          Id = I.Id,
                          Name = I.Name
                      };


            var modelos = await res.ToListAsync();

            if (modelos != null)
                return new(Responses.Success, modelos);

            return new(Responses.NotRows);


        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();




    }


    /// <summary>
    /// Actualizar la información de un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="name">Nuevo nombre.</param>
    /// <param name="description">Nueva descripción.</param>
    public async Task<ResponseBase> Update(int id, string name, string description)
    {

        // Ejecución
        try
        {

            var res = await (from I in context.Inventarios
                             where I.Id == id
                             select I).ExecuteUpdateAsync(t => t.SetProperty(a => a.Name, name).SetProperty(a => a.Direction, description));


            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();

    }


    /// <summary>
    /// Obtiene un inventario.
    /// </summary>
    /// <param name="id">Id del producto</param>
    public async Task<ReadOneResponse<int>> FindByProduct(int id)
    {

        // Ejecución
        try
        {

            var res = await (from p in context.Productos
                             where p.Id == id
                             select p.InventoryId).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == 0)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtiene un inventario,
    /// </summary>
    /// <param name="id">Id del producto detalle</param>
    public async Task<ReadOneResponse<int>> FindByProductDetail(int id)
    {

        // Ejecución
        try
        {

            var res = await (from p in context.ProductoDetalles
                             where p.Id == id
                             select p.Product.InventoryId).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == 0)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }

}