namespace LIN.Inventory.Persistence.Repositories;

public interface IInventoryAccessRepository
{
    Task<CreateResponse> Create(InventoryAccessDataModel model);
    Task<ResponseBase> DeleteSomeOne(int inventario, int profile);
    Task<ReadOneResponse<Notificacion>> Read(int id);
    Task<ReadAllResponse<Notificacion>> ReadAll(int id);
    Task<ReadAllResponse<Tuple<InventoryAccessDataModel, ProfileModel>>> ReadMembers(int inventario);
    Task<ResponseBase> UpdateRol(int id, InventoryRoles rol);
    Task<ResponseBase> UpdateState(int id, InventoryAccessState estado);
}