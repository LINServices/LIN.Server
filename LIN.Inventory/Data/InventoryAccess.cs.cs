namespace LIN.Inventory.Data;


public partial class InventoryAccess
{


    /// <summary>
    /// Crear acceso a inventario.
    /// </summary>
    /// <param name="model">Modelo.</param>
    /// <param name="context">Contexto de base de datos.</param>
    public async static Task<CreateResponse> Create(InventoryAcessDataModel model, Conexión context)
    {

        // Ejecución
        try
        {
            // Consultar si ya existe.
            var exist = await (from AI in context.DataBase.AccesoInventarios
                               where AI.ProfileID == model.ProfileID
                               && AI.Inventario == model.Inventario
                               select AI.ID).FirstOrDefaultAsync();

            // Si ya existe.
            if (exist > 0)
                return new()
                {
                    LastID = exist,
                    Response = Responses.ResourceExist
                };

            model.ID = 0;

            await context.DataBase.AccesoInventarios.AddAsync(model);

            context.DataBase.SaveChanges();

            return new(Responses.Success)
            {
                LastID = model.ID
            };


        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtener las invitaciones de un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="context">Contexto de base de datos.</param>
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
                          //UsuarioInvitador = U.Id,
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
    /// Obtener una invitación.
    /// </summary>
    /// <param name="id">Id de la invitación.</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<Notificacion>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.DataBase.AccesoInventarios
                      where AI.ID == id && AI.State == InventoryAccessState.OnWait
                      join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                      join U in context.DataBase.Profiles on I.Creador equals U.ID
                      select new Notificacion()
                      {
                          ID = AI.ID,
                          Fecha = AI.Fecha,
                          Inventario = I.Nombre,
                          //UsuarioInvitador = U.Id,
                          InventarioID = I.ID
                      };


            var modelos = await res.FirstOrDefaultAsync();
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
    /// Cambia el estado de una invitación.
    /// </summary>
    /// <param name="id">Id de la invitación</param>
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
    /// Obtiene la lista de integrantes de un inventario.
    /// </summary>
    /// <param name="inventario">Id del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<Tuple<InventoryAcessDataModel, ProfileModel>>> ReadMembers(int inventario, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.DataBase.AccesoInventarios
                      where AI.Inventario == inventario
                       &&( AI.State == InventoryAccessState.Accepted 
                       || AI.State == InventoryAccessState.OnWait)
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
    /// Eliminar a alguien de un inventario.
    /// </summary>
    /// <param name="inventario">Id del inventario.</param>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> DeleteSomeOne(int inventario, int profile, Conexión context)
    {

        // Ejecución
        try
        {

            // Actualizar estado.
            var result = await (from AI in context.DataBase.AccesoInventarios
                                where AI.Inventario == inventario
                                where AI.ProfileID == profile
                                select AI).ExecuteUpdateAsync(t => t.SetProperty(t => t.State, InventoryAccessState.Deleted).
                                                                   SetProperty(t => t.Rol, InventoryRoles.Banned));

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Cambia el rol.
    /// </summary>
    /// <param name="id">Id de la invitación</param>
    /// <param name="rol">Nuevo rol</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateRol(int id, InventoryRoles rol, Conexión context)
    {

        // Ejecución
        try
        {

            // Actualizar rol.
            var result = await (from AI in context.DataBase.AccesoInventarios
                                where AI.ID == id
                                && AI.State == InventoryAccessState.Accepted
                                select AI).ExecuteUpdateAsync(t => t.SetProperty(t => t.Rol, rol));

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError("Grave-- " + ex.Message);
        }

        return new();
    }



}