namespace LIN.Inventory.Hubs;

public interface IHubService
{
    Task SendNotification(int profile, int lastId);
    Task SendUpdateProduct(int inventoryId, int productId);
}