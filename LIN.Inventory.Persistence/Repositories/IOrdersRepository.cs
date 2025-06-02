namespace LIN.Inventory.Persistence.Repositories;

public interface IOrdersRepository
{
    Task<CreateResponse> Create(OrderModel model);
    Task<ReadAllResponse<OrderModel>> ReadAll(string externalId);
    Task<ReadOneResponse<OrderModel>> ReadByHold(int holdGroupId);
    Task<ResponseBase> Update(int id, string status);
    Task<ReadOneResponse<bool>> HasMovements(int order);
}