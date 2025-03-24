namespace LIN.Inventory.Persistence.Repositories;

public interface IStatisticsRepository
{
    Task<ReadOneResponse<decimal>> Sales(int profile, DateTime initDate, DateTime endDate);
    Task<ReadAllResponse<SalesModel>> SalesOn(int profile, DateTime initDate, DateTime endDate);
}