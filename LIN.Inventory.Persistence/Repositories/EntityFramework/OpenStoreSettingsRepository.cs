namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class OpenStoreSettingsRepository(Context.Context context) : IOpenStoreSettingsRepository
{

    /// <summary>
    /// Crear nueva configuración para Open Store.
    /// </summary>
    /// <param name="model">Modelo.</param>
    public async Task<CreateResponse> Create(OpenStoreSettings model)
    {

        // Modelo.
        model.InventoryDataModel = new() { Id = model.InventoryId };

        try
        {
            // Modelo ya existe.
            model.InventoryDataModel = context.AttachOrUpdate(model.InventoryDataModel);

            // Guardar ajustes.
            context.OpenStoreSettings.Add(model);
            await context.SaveChangesAsync();

            return new(Responses.Success, model.Id);
        }
        catch (Exception)
        {
        }
        return new();
    }

}