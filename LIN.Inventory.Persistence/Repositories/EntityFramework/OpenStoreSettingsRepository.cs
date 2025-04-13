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



    public async Task<ReadOneResponse<OpenStoreSettings>> Read(int id)
    {
        try
        {
            // Guardar ajustes.
            var z = await (from a in context.OpenStoreSettings
                           where a.InventoryId == id
                           select a).FirstOrDefaultAsync();

            if (z is null)
                return new(Responses.NotRows);

            return new(Responses.Success, z);
        }
        catch (Exception)
        {
        }
        return new();
    }

}