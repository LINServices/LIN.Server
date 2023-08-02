namespace LIN.Server.Data;


public class MailLinks
{


    #region Abstracciones


    /// <summary>
    /// Crea un nuevo LINK
    /// </summary>
    /// <param name="data">Modelo del link</param>
    public async static Task<CreateResponse> Create(EmailLink data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene un link activo según su key
    /// </summary>
    /// <param name="value"></param>
    public async static Task<ReadOneResponse<EmailLink>> ReadAndDisable(string value)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadAndDisable(value, context);
        context.CloseActions(connectionKey);
        return res;

    }



    #endregion



    /// <summary>
    /// Crea un nuevo enlace para email
    /// </summary>
    /// <param name="data">Modelo del link</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public async static Task<CreateResponse> Create(EmailLink data, Conexión context)
    {
        // ID en 0
        data.ID = 0;

        // Ejecución
        try
        {
            var res = context.DataBase.EmailLinks.Add(data);
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
    /// Obtiene un link activo según su key
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public async static Task<ReadOneResponse<EmailLink>> ReadAndDisable(string key, Conexión context)
    {

        // Ejecución
        try
        {

            var now = DateTime.Now;
            var verification = await (from L in context.DataBase.EmailLinks
                                     where L.Key == key
                                     where L.Vencimiento > now
                                     where L.Status == DBModels.ChangeLinkStatus.Actived
                                     select L).FirstOrDefaultAsync();


            
            if (verification == null)
            {
                return new();
            }

            verification.Status = ChangeLinkStatus.Desactived;
            context.DataBase.SaveChanges();

            return new(Responses.Success, verification);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



}