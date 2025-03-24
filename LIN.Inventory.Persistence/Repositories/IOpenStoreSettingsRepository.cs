namespace LIN.Inventory.Persistence.Repositories;

public interface IOpenStoreSettingsRepository
{
    Task<CreateResponse> Create(OpenStoreSettings model);
}