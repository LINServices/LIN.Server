namespace LIN.Inventory.Data;

public class Profiles(Context context, ILogger<Profiles> logger)
{

    /// <summary>
    /// Crear nuevo perfil.
    /// </summary>
    /// <param name="data">Data.</param>
    public async Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data)
    {

        data.Profile.Id = 0;

        // Ejecución (Transacción)
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                context.Profiles.Add(data.Profile);
                context.SaveChanges();

                // Creación del inventario.
                InventoryDataModel inventario = new()
                {
                    Creador = data.Profile.Id,
                    Nombre = "Mi Inventario",
                    Direction = $"Inventario personal de {data.Account.Identity.Unique}"
                };

                // Guardar inventario.
                await context.Inventarios.AddAsync(inventario);
                context.SaveChanges();

                // Acceso a inventario.
                InventoryAcessDataModel acceso = new()
                {
                    Fecha = DateTime.Now,
                    Inventario = inventario.ID,
                    State = InventoryAccessState.Accepted,
                    Rol = InventoryRoles.Administrator,
                    ProfileID = data.Profile.Id
                };

                context.AccesoInventarios.Add(acceso);
                context.SaveChanges();
                transaction.Commit();


                return new(Responses.Success, data.Profile);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.LogWarning(ex, "Error");
                if ((ex.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") ?? false) || (ex.InnerException?.Message.Contains("duplicate key") ?? false))
                    return new(Responses.ExistAccount);
            }
        }
        return new();
    }


    /// <summary>
    /// Obtener un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async Task<ReadOneResponse<ProfileModel>> Read(int id)
    {

        // Ejecución
        try
        {

            var res = await Query.Profiles.Read(id, context).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtener perfiles.
    /// </summary>
    /// <param name="ids">Id de los perfiles.</param>
    public async Task<ReadAllResponse<ProfileModel>> Read(List<int> ids)
    {

        // Ejecución
        try
        {

            var res = await Query.Profiles.Read(ids, context).ToListAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtener perfiles.
    /// </summary>
    /// <param name="ids">Id de las cuentas.</param>
    public async Task<ReadAllResponse<ProfileModel>> ReadByAccounts(List<int> ids)
    {

        // Ejecución
        try
        {

            var res = await Query.Profiles.ReadByAccounts(ids, context).ToListAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtener perfil.
    /// </summary>
    /// <param name="id">Id de la cuenta.</param>
    public async Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id)
    {

        // Ejecución
        try
        {

            var res = await Query.Profiles.ReadByAccount(id, context).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }

}