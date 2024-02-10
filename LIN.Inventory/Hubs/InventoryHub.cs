﻿using LIN.Types.Inventory.Transient;

namespace LIN.Inventory.Hubs;


public class InventoryHub : Hub
{


    public static Dictionary<int, List<DeviceModel>> List { get; set; } = [];



    /// <summary>
    /// Agregar una conexión a su grupo de cuenta.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    public async Task Join(string token, DeviceModel model)
    {

        // Información del token.
        var tokenInfo = Jwt.Validate(token);

        // Si el token es invalido.
        if (!tokenInfo.IsAuthenticated)
            return;


        var exist = List.ContainsKey(tokenInfo.ProfileId);

        if (!exist)
        {
            List.Add(tokenInfo.ProfileId, [new DeviceModel()
            {
                Id = Context.ConnectionId,
                Name = model.Name,
                Platform = model.Platform,
            }]);
        }

        model.Id = Context.ConnectionId;
        List[tokenInfo.ProfileId].Add(model);

        // Agregar el grupo.
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group.{tokenInfo.ProfileId}");

    }



    /// <summary>
    /// Enviar un comando a los demás dispositivos.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="command">Comando.</param>
    public async Task SendCommand(string token, CommandModel comando)
    {

        // Información del token.
        var tokenInfo = Jwt.Validate(token);

        // Si el token es invalido.
        if (!tokenInfo.IsAuthenticated)
            return;

        // Envía el comando.
        await Clients.GroupExcept($"group.{tokenInfo.ProfileId}", [Context.ConnectionId]).SendAsync("#command", comando);

    }



    public async Task SendToDevice(string device, CommandModel command)
    {

        // Envía el comando.
        await Clients.Client(device).SendAsync("#command", command);

    }





    /// <summary>
    /// Evento: Cuando un dispositivo se desconecta
    /// </summary>
    /// <param name="exception">Excepción</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {

            // Obtiene la sesión por el dispositivo
            var x = List.Values.FirstOrDefault(t => t.Any(t => t.Id == Context.ConnectionId));

            x?.RemoveAll(t => t.Id == Context.ConnectionId);

        }
        catch
        {
        }
    }


}