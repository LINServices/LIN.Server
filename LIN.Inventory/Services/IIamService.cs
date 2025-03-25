namespace LIN.Inventory.Services;

public interface IIamService
{
    Task<bool> CanAccept(int id, int profile);
    Task<InventoryRoles> Validate(IamRequest request);
}