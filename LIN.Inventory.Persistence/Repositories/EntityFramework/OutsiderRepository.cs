namespace LIN.Inventory.Persistence.Repositories.EntityFramework;

internal class OutsiderRepository(Context.Context context) : IOutsiderRepository
{

    /// <summary>
    /// Crear nuevo tercero.
    /// </summary>
    /// <param name="model">Modelo.</param>
    public async Task<CreateResponse> Create(OutsiderModel model)
    {

        try
        {
            model.Name = model.Name?.Trim() ?? string.Empty;

            // Existe el detalle.
            model.InventoryDataModel = context.AttachOrUpdate(model.InventoryDataModel);

            // Guardar el modelo.
            context.Outsider.Add(model);
            await context.SaveChangesAsync();

            return new(Responses.Success, model.Id);
        }
        catch (Exception)
        {
        }

        return new();
    }


    /// <summary>
    /// Obtener los terceros que concuerden con el documento.
    /// </summary>
    /// <param name="document">Documento.</param>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadAllResponse<OutsiderModel>> FindAllMatch(string document, int inventory)
    {

        try
        {
            var outsiders = await (from o in context.Outsider
                                   where o.InventoryId == inventory
                                   && o.Document.Contains(document)
                                   select o).ToListAsync();

            return new(Responses.Success, outsiders);
        }
        catch (Exception)
        {
        }

        return new();
    }


    /// <summary>
    /// Encontrar un tercero por el documento.
    /// </summary>
    /// <param name="document">Documento.</param>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadOneResponse<OutsiderModel>> FindByDocument(string document, int inventory)
    {
        try
        {
            var outsider = await (from o in context.Outsider
                                  where o.InventoryId == inventory
                                  && o.Document == document
                                  select o).FirstOrDefaultAsync();

            if (outsider is null)
                return new(Responses.NotRows);

            return new(Responses.Success, outsider);
        }
        catch (Exception)
        {
        }

        return new();
    }


    /// <summary>
    /// Buscar terceros.
    /// </summary>
    /// <param name="text">Texto para buscar.</param>
    /// <param name="inventory">Id del inventario.</param>
    public async Task<ReadAllResponse<OutsiderModel>> Search(string text, int inventory)
    {
        try
        {
            var outsiders = await (from o in context.Outsider
                                   where o.InventoryId == inventory
                                   &&
                                   (o.Email.Contains(text) ||
                                   o.Document.Contains(text) ||
                                   o.Name.Contains(text))
                                   select o).ToListAsync();

            return new(Responses.Success, outsiders);
        }
        catch (Exception)
        {
        }

        return new();
    }


    public async Task<CreateResponse> UpdateClean(OutsiderModel model)
    {

        try
        {

            var a = await (from x in context.Outsider
                     where x.Id == model.Id
                     select x).ExecuteUpdateAsync(t => 
                                                  t.SetProperty(t => t.Email, t => string.IsNullOrWhiteSpace(t.Email) ? model.Email : t.Email).
                                                  SetProperty(t => t.Name, t => string.IsNullOrWhiteSpace(t.Name) ? model.Name : t.Name).
                                                  SetProperty(t => t.Document, t => string.IsNullOrWhiteSpace(t.Document) ? model.Document : t.Document));


            var ss = await (from x in context.Outsider
                      where x.Id == model.Id
                      select x).FirstOrDefaultAsync();

            model.Email = ss.Email;
            model.Document = ss.Document;
            model.Name = ss.Name;

            // Guardar el modelo.
            await context.SaveChangesAsync();

            return new(Responses.Success, model.Id);
        }
        catch (Exception)
        {
        }

        return new();
    }


}