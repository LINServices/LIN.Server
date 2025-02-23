using LIN.Types.Inventory.Enumerations;
using LIN.Types.Inventory.Models;
using LIN.Types.Inventory.Transient;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace LIN.Inventory.Persistence.Data;

public class InventoryAccess(Context.Context context, ILogger<InventoryAccess> logger)
{

    /// <summary>
    /// Crear acceso a inventario.
    /// </summary>
    /// <param name="model">Modelo.</param>
    public async Task<CreateResponse> Create(InventoryAccessDataModel model)
    {

        // Ejecución
        try
        {
            // Consultar si ya existe.
            var exist = await (from AI in context.AccesoInventarios
                               where AI.ProfileId == model.ProfileId
                               && AI.Inventario == model.Inventario
                               select AI.Id).FirstOrDefaultAsync();

            // Si ya existe.
            if (exist > 0)
                return new()
                {
                    LastId = exist,
                    Response = Responses.ResourceExist
                };

            model.Id = 0;

            await context.AccesoInventarios.AddAsync(model);

            context.SaveChanges();

            return new(Responses.Success)
            {
                LastId = model.Id
            };


        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtener las invitaciones de un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async Task<ReadAllResponse<Notificacion>> ReadAll(int id)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.AccesoInventarios
                      where AI.ProfileId == id && AI.State == InventoryAccessState.OnWait
                      join I in context.Inventarios on AI.Inventario equals I.Id
                      join U in context.Profiles on I.Creador equals U.Id
                      select new Notificacion()
                      {
                          ID = AI.Id,
                          Fecha = AI.Fecha,
                          Inventario = I.Nombre,
                          //UsuarioInvitador = U.Id,
                          InventarioID = I.Id
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
    /// Obtener una invitación.
    /// </summary>
    /// <param name="id">Id de la invitación.</param>
    public async Task<ReadOneResponse<Notificacion>> Read(int id)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.AccesoInventarios
                      where AI.Id == id && AI.State == InventoryAccessState.OnWait
                      join I in context.Inventarios on AI.Inventario equals I.Id
                      join U in context.Profiles on I.Creador equals U.Id
                      select new Notificacion()
                      {
                          ID = AI.Id,
                          Fecha = AI.Fecha,
                          Inventario = I.Nombre,
                          //UsuarioInvitador = U.Id,
                          InventarioID = I.Id
                      };


            var modelos = await res.FirstOrDefaultAsync();
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
    /// Cambia el estado de una invitación.
    /// </summary>
    /// <param name="id">Id de la invitación</param>
    /// <param name="estado">Nuevo estado</param>
    public async Task<ResponseBase> UpdateState(int id, InventoryAccessState estado)
    {

        // Ejecución
        try
        {
            var model = await context.AccesoInventarios.FindAsync(id);

            if (model != null)
            {
                model.State = estado;
                context.SaveChanges();
                return new(Responses.Success);
            }

            return new(Responses.NotRows);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Obtiene la lista de integrantes de un inventario.
    /// </summary>
    /// <param name="inventario">Id del inventario</param>
    public async Task<ReadAllResponse<Tuple<InventoryAccessDataModel, ProfileModel>>> ReadMembers(int inventario)
    {

        // Ejecución
        try
        {

            // Consulta
            var res = from AI in context.AccesoInventarios
                      where AI.Inventario == inventario
                       && (AI.State == InventoryAccessState.Accepted
                       || AI.State == InventoryAccessState.OnWait)
                      join U in context.Profiles on AI.ProfileId equals U.Id
                      select new Tuple<InventoryAccessDataModel, ProfileModel>(AI, U);


            var modelos = await res.ToListAsync();

            if (modelos == null)
                return new(Responses.NotRows);

            return new(Responses.Success, modelos);


        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Eliminar a alguien de un inventario.
    /// </summary>
    /// <param name="inventario">Id del inventario.</param>
    /// <param name="profile">Id del perfil.</param>
    public async Task<ResponseBase> DeleteSomeOne(int inventario, int profile)
    {

        // Ejecución
        try
        {

            // Actualizar estado.
            var result = await (from AI in context.AccesoInventarios
                                where AI.Inventario == inventario
                                where AI.ProfileId == profile
                                select AI).ExecuteUpdateAsync(t => t.SetProperty(t => t.State, InventoryAccessState.Deleted).
                                                                   SetProperty(t => t.Rol, InventoryRoles.Banned));

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }


    /// <summary>
    /// Cambia el rol.
    /// </summary>
    /// <param name="id">Id de la invitación</param>
    /// <param name="rol">Nuevo rol</param>
    public async Task<ResponseBase> UpdateRol(int id, InventoryRoles rol)
    {

        // Ejecución
        try
        {

            // Actualizar rol.
            var result = await (from AI in context.AccesoInventarios
                                where AI.Id == id
                                && AI.State == InventoryAccessState.Accepted
                                select AI).ExecuteUpdateAsync(t => t.SetProperty(t => t.Rol, rol));

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error");
        }

        return new();
    }

}