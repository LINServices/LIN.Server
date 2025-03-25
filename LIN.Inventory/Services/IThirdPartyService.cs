namespace LIN.Inventory.Services;

public interface IThirdPartyService
{
    Task<ReadOneResponse<OutsiderModel>> FindOrCreate(OutsiderModel model, int inventory);
}