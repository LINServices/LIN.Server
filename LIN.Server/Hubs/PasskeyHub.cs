namespace LIN.Server.Hubs;


public class PassKeyHub : Hub
{


    /// <summary>
    /// Lista de intentos Passkey
    /// </summary>
    public readonly static Dictionary<string, List<PasskeyIntentDataModel>> PassKeyIntents = new();





    public override Task OnDisconnectedAsync(Exception? exception)
    {

        var e = PassKeyIntents.Values.Where(T => T.Where(T => T.HubKey == Context.ConnectionId).Any()).FirstOrDefault() ?? new();


        _ = e.Where(T =>
          {
              if (T.HubKey == Context.ConnectionId && T.Status == PassKeyStatus.Undefined)
                  T.Status = PassKeyStatus.Failed;

              return false;
          });


        return base.OnDisconnectedAsync(exception);
    }



    /// <summary>
    /// Un dispositivo envia el PassKey intent
    /// </summary>
    public async Task JoinIntent(PasskeyIntentDataModel modelo)
    {

        var expiracion = DateTime.Now.AddMinutes(2);

        // Modelo
        modelo.HubKey = Context.ConnectionId;
        modelo.Status = PassKeyStatus.Undefined;
        modelo.Hora = DateTime.Now;
        modelo.Expiracion = expiracion;

        // Agrega el modelo
        if (!PassKeyIntents.ContainsKey(modelo.User.ToLower()))
            PassKeyIntents.Add(modelo.User.ToLower(), new() { modelo });
        else
            PassKeyIntents[modelo.User.ToLower()].Add(modelo);

        // Yo
        await Groups.AddToGroupAsync(Context.ConnectionId, $"dbo.{Context.ConnectionId}");

        await SendRequest(modelo);

    }



    /// <summary>
    /// Un dispositivo envia el PassKey intent
    /// </summary>
    public async Task JoinAdmin(string usuario)
    {

        // Grupo de la cuenta
        await Groups.AddToGroupAsync(Context.ConnectionId, usuario.ToLower());

    }







    //=========== Dispositivos ===========//


    /// <summary>
    /// Envia la solicitud a los admins
    /// </summary>
    public async Task SendRequest(PasskeyIntentDataModel modelo)
    {
        await Clients.Group(modelo.User.ToLower()).SendAsync("newintent", modelo);
    }




    /// <summary>
    /// 
    /// </summary>
    public async void ReceiveRequest(PasskeyIntentDataModel modelo)
    {

        try
        {
            // Obtiene la cuenta
            var cuenta = PassKeyIntents[modelo.User.ToLower()];

            // Obtiene el dispositivo
            var intent = cuenta.Where(T => T.HubKey == modelo.HubKey).ToList().FirstOrDefault();

            if (intent == null)
                return;

            intent.Status = modelo.Status;

            if (DateTime.Now > modelo.Expiracion)
            {
                intent.Status = PassKeyStatus.Expired;
                modelo.Status = PassKeyStatus.Expired;
                modelo.Token = string.Empty;
                intent.Token = string.Empty;
            }

            await Clients.Groups($"dbo.{modelo.HubKey}").SendAsync("recieveresponse", modelo);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError($"|{1}|On Hub Key: " + ex.Message);
        }



    }





}
