using LIN.Types.Cloud.Identity.Abstracts;

namespace LIN.Inventory.Persistence.Repositories;

public interface IProfilesRepository
{
    Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data);
    Task<ReadOneResponse<ProfileModel>> Read(int id);
    Task<ReadAllResponse<ProfileModel>> Read(List<int> ids);
    Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id);
    Task<ReadAllResponse<ProfileModel>> ReadByAccounts(List<int> ids);
}