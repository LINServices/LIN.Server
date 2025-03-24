namespace LIN.Inventory.Persistence.Repositories;

public interface IHoldsRepository
{
    Task<CreateResponse> Approve(int holdId);
    Task<CreateResponse> Create(HoldModel model, IDbContextTransaction? contextTransaction = null);
    Task<CreateResponse> Return(int holdId);
}