namespace LIN.Inventory.Services;

public class ThirdPartyService(IOutsiderRepository outsiderRepository)
{

    /// <summary>
    /// Encontrar o crear un tercero.
    /// </summary>
    /// <param name="model">Modelo del tercero.</param>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadOneResponse<OutsiderModel>> FindOrCreate(OutsiderModel model, int inventory)
    {

        // Intentar encontrar.
        var outsider = await outsiderRepository.FindByDocument(model.Document, inventory);

        // Si todo salió bien.
        if (outsider.Response == Responses.Success)
        {
            model.Id = outsider.Model.Id;
            await outsiderRepository.UpdateClean(model);
            return new(Responses.Success, model);
        }

        // Si no se encontró se crea.
        var responseCreate = await outsiderRepository.Create(new()
        {
            Document = model.Document,
            Name = model.Name,
            Type = model.Type,
            Email = model.Email,
            InventoryDataModel = new() { Id = inventory }
        });

        if (responseCreate.Response == Responses.Success)
        {
            model.Id = responseCreate.LastId;
            return new(Responses.Success, model);
        }

        return new(Responses.Undefined);
    }

}