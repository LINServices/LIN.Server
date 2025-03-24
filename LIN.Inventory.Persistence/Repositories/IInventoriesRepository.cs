namespace LIN.Inventory.Persistence.Repositories;

public interface IInventoriesRepository
{
    Task<CreateResponse> Create(InventoryDataModel data);
    Task<ReadOneResponse<int>> FindByProduct(int id);
    Task<ReadOneResponse<int>> FindByProductDetail(int id);
    Task<ReadOneResponse<InventoryDataModel>> Read(int id);
    Task<ReadAllResponse<InventoryDataModel>> ReadAll(int id);
    Task<ResponseBase> Update(int id, string name, string description);
}