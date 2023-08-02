namespace LIN.Server.Data;


public class ProductTemplate
{


    #region Abstracciones



    /// <summary>
    /// Crea una plantilla de producto
    /// </summary>
    /// <param name="data">Modelo de la plantilla</param>
    public async static Task<CreateResponse> Create(DBModels.ProductTemplateTable data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Actualiza la plantilla de producto
    /// </summary>
    /// <param name="data">Modelo de la plantilla</param>
    public async static Task<ResponseBase> Update(DBModels.ProductTemplateTable data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Update(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene la lista de plantillas según el parámetro
    /// </summary>
    /// <param name="parameter">Parámetro de búsqueda</param>
    public async static Task<ReadAllResponse<ProductDataTransfer>> ReadAllBy(string parameter)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await ReadAllBy(parameter, context);
        context.CloseActions(connectionKey);
        return res;

    }




    #endregion



    /// <summary>
    /// Crea una plantilla de producto
    /// </summary>
    /// <param name="data">Modelo del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(DBModels.ProductTemplateTable data, Conexión context)
    {
        data.ID = 0;
        try
        {
            // Agrega y guarda
            await context.DataBase.PlantillaProductos.AddAsync(data);
            context.DataBase.SaveChanges();

            return new(Responses.Success, data.ID);
        }
        catch
        {
            return new(Responses.Undefined);
        }

    }



    /// <summary>
    /// Actualiza la información de una plantilla
    /// </summary>
    /// <param name="data">Modelo de la plantilla</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Update(DBModels.ProductTemplateTable data, Conexión context)
    {

        try
        {

            // Plantilla
            var plantilla = await context.DataBase.PlantillaProductos.FindAsync(data.ID);

            // Evalúa la existencia
            if (plantilla == null)
            {
                return new(Responses.NotRows);
            }

            // Nuevos datos
            plantilla.Description = data.Description;
            plantilla.Category = data.Category;
            plantilla.Code = data.Code;
            plantilla.Name = data.Name;
            plantilla.Image = data.Image;

            // Save
            context.DataBase.SaveChanges();

            return new(Responses.Success);

        }
        catch
        {

        }
        return new();

    }



    /// <summary>
    /// Obtiene la cantidad de productos activos asociados a una plantilla
    /// </summary>
    /// <param name="plantilla">ID de la plantilla</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> HasProducts(int plantilla, Conexión context)
    {

        try
        {

            var count = await (from P in context.DataBase.PlantillaProductos
                               where P.ID == plantilla
                               join PP in context.DataBase.Productos
                               on P.ID equals PP.Plantilla
                               select PP).CountAsync();

            return new(Responses.Success, count);

        }
        catch
        {
        }

        return new();

    }



    /// <summary>
    /// Obtiene la lista de plantillas según el parámetro
    /// </summary>
    /// <param name="parameter">Parámetro de búsqueda</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<ProductDataTransfer>> ReadAllBy(string parameter, Conexión context)
    {

        try
        {

            parameter = parameter.ToLower();

            // Plantilla
            var plantillas = await (from P in context.DataBase.PlantillaProductos
                                    where P.Name.ToLower().Contains(parameter)
                                    || P.Code.ToLower().Contains(parameter)
                                    || P.Description.ToLower().Contains(parameter)
                                    select new ProductDataTransfer
                                    {
                                        Plantilla = P.ID,
                                        Name = P.Name,
                                        Category = P.Category,
                                        Code = P.Code,
                                        Description = P.Description,
                                        Image = P.Image
                                    }).Take(10).ToListAsync();


            // Evalúa la existencia
            if (plantillas == null)
            {
                return new(Responses.Undefined);
            }

            return new(Responses.Success, plantillas);

        }
        catch
        {

        }
        return new();

    }



}