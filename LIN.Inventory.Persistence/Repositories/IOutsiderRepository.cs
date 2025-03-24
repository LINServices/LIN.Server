namespace LIN.Inventory.Persistence.Repositories;

public interface IOutsiderRepository
{
    Task<CreateResponse> Create(OutsiderModel model);
    Task<ReadAllResponse<OutsiderModel>> FindAllMatch(string document, int inventory);
    Task<ReadOneResponse<OutsiderModel>> FindByDocument(string document, int inventory);
    Task<ReadAllResponse<OutsiderModel>> Search(string text, int inventory);
    Task<CreateResponse> UpdateClean(OutsiderModel model);
}