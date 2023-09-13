namespace LIN.Inventory.Data;


public class InventoryAccess
{



    #region Abstracciones


    /// <summary>
    /// Obtiene la lista de invitaciones a un inventario
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadAllResponse<Notificacion>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var rs = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return rs;
    }



    /// <summary>
    /// Cambia el estado de una invitación
    /// </summary>
    /// <param name="id">ID de la invitación</param>
    /// <param name="estado">Nuevo estado</param>
    public async static Task<ResponseBase> UpdateState(int id, InventoryAccessState estado)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await UpdateState(id, estado, context);
        context.CloseActions(connectionKey);
        return res;
    }


    /// <summary>
    /// Cambia el estado de una invitación
    /// </summary>
    /// <param name="id">ID de la invitación</param>
    /// <param name="rol">Nuevo rol</param>
    public async static Task<ResponseBase> UpdateRol(int id, int adminID, InventoryRoles rol)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await UpdateRol(id, adminID, rol, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene la lista de invitaciones a un inventario
    /// </summary>
    public async static Task<ReadAllResponse<Tuple<InventoryAcessDataModel, ProfileModel>>> ReadIntegrants(int inventario)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadIntegrants(inventario, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Elimina a alguien de un inventario
    /// </summary>
    public async static Task<ResponseBase> DeleteSomeOne(int inventario, int usuario, int me)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await DeleteSomeOne(inventario, usuario, me, context);
        context.CloseActions(connectionKey);
        return res;
    }


    /// <summary>
    /// Genera nuevas invitaciones para un inventario
    /// </summary>
    /// <param name="inventario">Modelo del inventario</param>
    [Obsolete("Esta función se esta probando (URGENTE)")]
    public async static Task<ResponseBase> GenerateInvitation(InventoryDataModel inventario)
    {
        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await GenerateInvitation(inventario, context);
        context.CloseActions(connectionKey);
        return res;
    }



    #endregion



    /// <summary>
    /// Obtiene la lista de invitaciones a un inventario que aun no han sido aceptadas
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<Notificacion>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.DataBase.AccesoInventarios
                      where AI.ProfileID == id && AI.State == InventoryAccessState.OnWait
                      join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                      join U in context.DataBase.Profiles on I.Creador equals U.ID
                      select new Notificacion()
                      {
                          ID = AI.ID,
                          Fecha = AI.Fecha,
                          Inventario = I.Nombre,
                          //UsuarioInvitador = U.ID,
                          InventarioID = I.ID
                      };


            var modelos = await res.ToListAsync();
            if (modelos != null)
                return new(Responses.Success, modelos);

            return new(Responses.NotRows);


        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Cambia el estado de una invitación
    /// </summary>
    /// <param name="id">ID de la invitación</param>
    /// <param name="estado">Nuevo estado</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateState(int id, InventoryAccessState estado, Conexión context)
    {

        // Ejecución
        try
        {
            var model = await context.DataBase.AccesoInventarios.FindAsync(id);

            if (model != null)
            {
                model.State = estado;
                context.DataBase.SaveChanges();
                return new(Responses.Success);
            }

            return new(Responses.NotRows);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError("Grave-- " + ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de integrantes de un inventario
    /// </summary>
    /// <param name="inventario">ID del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<Tuple<InventoryAcessDataModel, ProfileModel>>> ReadIntegrants(int inventario, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.DataBase.AccesoInventarios
                      where AI.Inventario == inventario && AI.State == InventoryAccessState.Accepted
                      join U in context.DataBase.Profiles on AI.ProfileID equals U.ID
                      select new Tuple<InventoryAcessDataModel, ProfileModel>(AI, U);


           var modelos = await res.ToListAsync();

            if (modelos == null)
                return new(Responses.NotRows);

            return new(Responses.Success, modelos);


        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Elimina a alguien de un inventario
    /// </summary>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> DeleteSomeOne(int inventario, int usuario, int me, Conexión context)
    {

        // Ejecución
        try
        {

            // Obtiene cual es el rol del usuario que inicio la operación
            var meRol = (from AI in context.DataBase.AccesoInventarios
                         where AI.ProfileID == me && AI.Inventario == inventario
                         select new
                         {
                             AI.Rol
                         }).FirstOrDefault();

            // Evalúa
            if (meRol == null)
            {
                return new();
            }


            // Si no tiene permisos
            if (meRol.Rol != InventoryRoles.Administrator)
            {
                return new(Responses.Unauthorized);
            }


            // Obtiene el ID del acceso inventario, al cual intentan eliminar
            var userID = (from AI in context.DataBase.AccesoInventarios
                          where AI.ProfileID == usuario && AI.Inventario == inventario && AI.State != InventoryAccessState.Deleted
                          select AI.ID).FirstOrDefault();

            // Evalúa
            if (userID == 0)
            {
                return new(Responses.Undefined);
            }

            // Obtiene el acceso completo
            var user = await context.DataBase.AccesoInventarios.FindAsync(userID);

            // Evalúa
            if (user == null)
                return new(Responses.Undefined);

            // Cambia el rol
            user.Rol = InventoryRoles.Banned;
            user.State = InventoryAccessState.Deleted;


            context.DataBase.SaveChanges();

            return new(Responses.Success);


        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Genera nuevas invitaciones para un inventario
    /// </summary>
    /// <param name="inventario">Modelo del inventario (ID y nuevos integrantes)</param>
    /// <param name="context">Contexto de conexión</param>
    [Obsolete("Esta función se esta probando (URGENTE)")]
    public async static Task<ResponseBase> GenerateInvitation(InventoryDataModel inventario, Conexión context)
    {

        // Ejecución
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                // Obtiene los actuales integrantes
                var actualIntegrants = (from AI in context.DataBase.AccesoInventarios
                                        where AI.Inventario == inventario.ID && AI.State != InventoryAccessState.Deleted
                                        select AI).ToList();

                // Fecha actual
                var time = DateTime.Now;

                // Opciones para el Parallel
                ParallelOptions options = new()
                {
                    MaxDegreeOfParallelism = 30,
                };

                // Foreach simultaneo
                Parallel.ForEach(inventario.UsersAccess, options, (@new, token) =>
                {
                    // Si ya existe un usuario con este (Username)
                    var have = actualIntegrants.Where(T => T.ProfileID == @new.ProfileID).Any();

                    if (have)
                        return;

                    // Nuevo acceso
                    var acceso = new InventoryAcessDataModel()
                    {
                        Fecha = time,
                        State = InventoryAccessState.OnWait,
                        ID = 0,
                        Inventario = inventario.ID,
                        Rol = @new.Rol,
                        ProfileID = @new.ProfileID
                    };
                    context.DataBase.Add(acceso);

                });

                // Guarda los cambios
                context.DataBase.SaveChanges();
                transaction.Commit();

                // Cierra la conexión
                return new ResponseBase(Responses.Success);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ServerLogger.LogError(ex.Message);
            }

        }
        return new();
    }



    /// <summary>
    /// Cambia el rol
    /// </summary>
    /// <param name="id">ID de la invitación</param>
    /// <param name="rol">Nuevo rol</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateRol(int id, int adminID, InventoryRoles rol, Conexión context)
    {

        // Ejecución
        try
        {

            var model = await context.DataBase.AccesoInventarios.FindAsync(id);

            if (model != null)
            {

                var acceso = await (from AI in context.DataBase.AccesoInventarios
                                    where AI.Inventario == model.Inventario
                                    where AI.ProfileID == adminID
                                    where AI.State == InventoryAccessState.Accepted
                                    select AI).FirstOrDefaultAsync();

                if (acceso == null || acceso.Rol != InventoryRoles.Administrator)
                {
                    return new(Responses.Unauthorized);
                }

                model.Rol = rol;
                context.DataBase.SaveChanges();
                return new(Responses.Success);
            }

            return new(Responses.NotRows);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError("Grave-- " + ex.Message);
        }

        return new();
    }



}
