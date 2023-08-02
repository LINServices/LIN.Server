namespace LIN.Server.Data;


public class Links
{


    #region Abstracciones



    /// <summary>
    /// Crea un nuevo LINK
    /// </summary>
    /// <param name="data">Modelo del link</param>
    public async static Task<CreateResponse> Create(ChangeLink data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene la lista de links asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadAllResponse<ChangeLink>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un link y cambia su estado
    /// </summary>
    /// <param name="value"></param>
    public async static Task<ReadOneResponse<ChangeLink>> ReadOneAnChange(string value)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadOneAnChange(value, context);
        context.CloseActions(connectionKey);
        return res;

    }



    #endregion



    /// <summary>
    /// Crea un nuevo enlace
    /// </summary>
    /// <param name="data">Modelo del enlace</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(ChangeLink data, Conexión context)
    {
        // ID en 0
        data.ID = 0;

        // Ejecución
        try
        {
            var res = context.DataBase.Links.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch (Exception ex)
        {
            context.DataBase.Remove(data);
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



    /// <summary>
    /// Obtiene la lista de links asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<ChangeLink>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var now = DateTime.Now;

            var activos = await (from L in context.DataBase.Links
                                 where L.User == id
                                 where L.Vencimiento > now
                                 where L.Status == ChangeLinkStatus.Actived
                                 select L).ToListAsync();

            var lista = activos;

            return new(Responses.Success, lista);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ChangeLink>> ReadOneAnChange(string value, Conexión context)
    {

        // Ejecución
        try
        {

            var now = DateTime.Now;

            var elemento = await (from L in context.DataBase.Links
                                  where L.Vencimiento > now
                                  where L.Status == ChangeLinkStatus.Actived
                                  where L.Key == value
                                  select L).FirstOrDefaultAsync();

            if (elemento == null)
            {
                return new();
            }

            elemento.Status = ChangeLinkStatus.None;
            context.DataBase.SaveChanges();





            return new(Responses.Success, elemento);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return new();
    }



}