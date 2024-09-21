using LIN.Inventory.Data;

namespace LIN.Inventory.Hubs;


public class HubService(IHubContext<InventoryHub> hubContext, Context context) : IHubService
{

    const string GroupName = "group.{0}";


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
            Command = command
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
                         where i.Inventario == inventory
                         where i.State == InventoryAccessState.OnWait
                         select new
                         {
                             Profile = i.ProfileID,
                             Id = i.ID,
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

}