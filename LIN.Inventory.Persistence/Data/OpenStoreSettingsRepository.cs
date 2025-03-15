using LIN.Types.Inventory.Models;
using LIN.Types.Responses;
using Microsoft.Extensions.Logging;

namespace LIN.Inventory.Persistence.Data;

public class OpenStoreSettingsRepository(Context.Context context, ILogger<OpenStoreSettingsRepository> logger)
{


    public async Task<CreateResponse> Create(OpenStoreSettings model)
    {
        model.InventoryDataModel = new() { Id = model.InventoryId };

        try
        {

            context.Attach(model.InventoryDataModel);

            context.OpenStoreSettings.Add(model);
            await context.SaveChangesAsync();

            return new(Responses.Success, model.Id);


        }
        catch (Exception ex)
        {

        }
        return new();

    }







}