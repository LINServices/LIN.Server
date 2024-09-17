namespace LIN.Inventory.Hubs;


public class HubService(IHubContext<InventoryHub> hubContext) : IHubService
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

}