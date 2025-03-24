namespace LIN.Inventory.Persistence.Repositories;

public interface IProductsRepository
{
    Task<CreateResponse> Create(ProductModel data);
    Task<ResponseBase> Delete(int id);
    Task<ReadOneResponse<ProductModel>> Read(int id);
    Task<ReadAllResponse<ProductModel>> ReadAll(int id);
    Task<ReadOneResponse<ProductModel>> ReadByDetail(int id);
    Task<ResponseBase> Update(ProductModel data);
    Task<ResponseBase> UpdateBase(ProductModel data);
    Task<ResponseBase> UpdateDetail(int id, ProductDetailModel data);
}