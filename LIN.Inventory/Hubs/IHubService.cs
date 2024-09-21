namespace LIN.Inventory.Hubs;

public interface IHubService
{
    Task SendNotification(int profile, int lastId);
    Task SendUpdateProduct(int inventoryId, int productId);
    Task SendNewProduct(int inventoryId, int productId);
    Task SendDeleteProduct(int inventoryId, int productId);
    Task SendNotification(int inventory);
    Task SendInflowMovement(int inventoryId, int movement);
    Task SendOutflowMovement(int inventoryId, int movement);
}