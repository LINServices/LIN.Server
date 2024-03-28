namespace LIN.Inventory.Data;

public partial class Profiles
{


    /// <summary>
    /// Crear nuevo perfil.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="context">Contexto de base de datos.</param>
    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data, Conexión context)
    {

        data.Profile.ID = 0;

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                context.DataBase.Profiles.Add(data.Profile);
                context.DataBase.SaveChanges();

                // Creación del inventario
                InventoryDataModel inventario = new()
                {
                    Creador = data.Profile.ID,
                    Nombre = "Mi Inventario",
                    Direction = $"Inventario personal de {data.Account.Identity.Unique}"
                };

                await context.DataBase.Inventarios.AddAsync(inventario);
                context.DataBase.SaveChanges();

                // Acceso a inventario
                InventoryAcessDataModel acceso = new()
                {
                    Fecha = DateTime.Now,
                    Inventario = inventario.ID,
                    State = InventoryAccessState.Accepted,
                    Rol = InventoryRoles.Administrator,
                    ProfileID = data.Profile.ID
                };

                context.DataBase.AccesoInventarios.Add(acceso);
                context.DataBase.SaveChanges();
                transaction.Commit();


                return new(Responses.Success, data.Profile);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ServerLogger.LogError(ex.Message);
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
    /// <param name="context">Contexto de base de datos.</param>
    public async static Task<ReadOneResponse<ProfileModel>> Read(int id, Conexión context)
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
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtener perfiles.
    /// </summary>
    /// <param name="ids">Id de los perfiles.</param>
    /// <param name="context">Contexto de base de datos.</param>
    public async static Task<ReadAllResponse<ProfileModel>> Read(List<int> ids, Conexión context)
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
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtener perfiles.
    /// </summary>
    /// <param name="ids">Id de los perfiles.</param>
    /// <param name="context">Contexto de base de datos.</param>
    public async static Task<ReadAllResponse<ProfileModel>> ReadByAccounts(List<int> ids, Conexión context)
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
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtener perfil.
    /// </summary>
    /// <param name="id">Id de la cuenta.</param>
    /// <param name="context">Contexto de base de datos.</param>
    public async static Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id, Conexión context)
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
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



}