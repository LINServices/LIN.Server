namespace LIN.Inventory.Persistence.Repositories;

public interface IOutflowsRepository
{
    Task<CreateResponse> Create(OutflowDataModel data, bool updateInventory = true);
    Task<ReadAllResponse<OutflowRow>> Informe(int month, int year, int inventory);
    Task<ReadOneResponse<OutflowDataModel>> Read(int id, bool includeDetails);
    Task<ReadAllResponse<OutflowDataModel>> ReadAll(int id);
    Task<CreateResponse> Reverse(int order);
    Task<ResponseBase> Update(int id, DateTime date);
    Task<CreateResponse> ReverseOutflow(int outflowId);
    Task<ReadOneResponse<int>> GetInventory(int id);
}