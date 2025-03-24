namespace LIN.Inventory.Persistence.Repositories;

public interface IInflowsRepository
{
    Task<ReadOneResponse<int>> ComprasOf(int id, int days);
    Task<CreateResponse> Create(InflowDataModel data);
    Task<ReadAllResponse<InflowRow>> Informe(int month, int year, int inventory);
    Task<ReadOneResponse<InflowDataModel>> Read(int id, bool includeDetails);
    Task<ReadAllResponse<InflowDataModel>> ReadAll(int id);
    Task<ResponseBase> Update(int id, DateTime date);
    Task<ReadOneResponse<int>> GetInventory(int id);
}