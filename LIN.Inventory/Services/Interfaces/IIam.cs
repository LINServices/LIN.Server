namespace LIN.Inventory.Services.Interfaces;

public interface IIam
{
    Task<bool> CanAccept(int id, int profile);
    Task<InventoryRoles> Validate(IamRequest request);
}