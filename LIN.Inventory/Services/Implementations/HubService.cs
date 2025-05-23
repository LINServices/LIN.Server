﻿namespace LIN.Inventory.Services.Implementations;

public class HubService(IHubContext<InventoryHub> hubContext, Persistence.Context.Context context) : IHubService
{

    /// <summary>
    /// Nombre de los grupos.
    /// </summary>
    private const string GroupName = "group.{0}";


    /// <summary>
    /// Enviar notificación.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="lastId">Id de la invitación.</param>
    public async Task SendNotification(int profile, int lastId)
    {
        // Servicio.
        string groupName = string.Format(GroupName, profile);
        string command = $"newInvitation({lastId})";

        // Enviar en tiempo real.
        await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
        {
            Command = command
        });
    }


    /// <summary>
    /// Enviar notificación de actualización de producto.
    /// </summary>
    public async Task SendUpdateProduct(int inventoryId, int productId)
    {
        // Realtime.
        string groupName = $"inventory.{inventoryId}";
        string command = $"updateProduct({productId})";
        await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
        {
            Command = command,
            Inventory = inventoryId
        });
    }


    /// <summary>
    /// Enviar notificación de nuevo de producto.
    /// </summary>
    public async Task SendNewProduct(int inventoryId, int productId)
    {
        // Realtime.
        string groupName = $"inventory.{inventoryId}";
        string command = $"addProduct({productId})";
        await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
        {
            Command = command
        });
    }


    /// <summary>
    /// Enviar notificación de eliminar producto.
    /// </summary>
    public async Task SendDeleteProduct(int inventoryId, int productId)
    {
        // Realtime.
        string groupName = $"inventory.{inventoryId}";
        string command = $"deleteProduct({productId})";
        await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
        {
            Command = command
        });
    }


    /// <summary>
    /// Enviar notificación.
    /// </summary>
    /// <param name="inventory">Id del nuevo inventario.</param>
    public async Task SendNotification(int inventory)
    {

        var ids = await (from i in context.AccesoInventarios
                         where i.InventoryId == inventory
                         where i.State == InventoryAccessState.OnWait
                         select new
                         {
                             Profile = i.ProfileId,
                             i.Id,
                         }).ToListAsync();


        foreach (var id in ids)
        {
            string groupName = $"group.{id.Profile}";
            string command = $"newInvitation({id.Id})";

            await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
            {
                Command = command
            });
        }
    }


    /// <summary>
    /// Enviar notificación de nueva entrada.
    /// </summary>
    public async Task SendInflowMovement(int inventoryId, int movement)
    {
        // Realtime.
        string groupName = $"inventory.{inventoryId}";
        string command = $"addInflow({movement})";
        await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
        {
            Command = command,
            Inventory = inventoryId
        });
    }


    /// <summary>
    /// Enviar notificación de nueva salida.
    /// </summary>
    public async Task SendOutflowMovement(int inventoryId, int movement)
    {
        // Realtime.
        string groupName = $"inventory.{inventoryId}";
        string command = $"addOutflow({movement})";
        await hubContext.Clients.Group(groupName).SendAsync("#command", new CommandModel()
        {
            Command = command,
            Inventory = inventoryId
        });
    }

}