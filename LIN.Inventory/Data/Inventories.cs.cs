namespace LIN.Inventory.Data;


public partial class Inventories
{


    /// <summary>
    /// Crea un nuevo inventario.
    /// </summary>
    /// <param name="data">Modelo del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(InventoryDataModel data, Conexión context)
    {

        // Modelo
        data.ID = 0;

        // Transacción
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {

                // InventoryId
                context.DataBase.Inventarios.Add(data);

                // Guarda el inventario
                await context.DataBase.SaveChangesAsync();


                // Accesos
                DateTime dateTime = DateTime.Now;
                foreach (var acceso in data.UsersAccess)
                {
                    // Propiedades
                    acceso.ID = 0;
                    acceso.Fecha = dateTime;
                    acceso.Inventario = data.ID;

                    // Accesos
                    context.DataBase.AccesoInventarios.Add(acceso);

                }

                // Guarda los cambios
                await context.DataBase.SaveChangesAsync();

                // Finaliza
                transaction.Commit();
                return new(Responses.Success, data.ID);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                context.DataBase.Remove(data);
                ServerLogger.LogError(ex.Message);
            }
        }


        return new();
    }



    /// <summary>
    /// Obtiene un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<InventoryDataModel>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {
            var res = await context.DataBase.Inventarios.FirstOrDefaultAsync(T => T.ID == id);

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de inventarios asociados a un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<InventoryDataModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = from AI in context.DataBase.AccesoInventarios
                      where AI.ProfileID == id && AI.State == InventoryAccessState.Accepted
                      join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                      select new InventoryDataModel()
                      {
                          MyRol = AI.Rol,
                          Creador = I.Creador,
                          Direction = I.Direction,
                          ID = I.ID,
                          Nombre = I.Nombre,
                          UsersAccess = I.UsersAccess
                      };


            var modelos = await res.ToListAsync();

            if (modelos != null)
                return new(Responses.Success, modelos);

            return new(Responses.NotRows);


        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();




    }



    /// <summary>
    /// Actualizar la información de un inventario.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="name">Nuevo nombre.</param>
    /// <param name="description">Nueva descripción.</param>
    /// <param name="context">Contexto de conexión..</param>
    public async static Task<ResponseBase> Update(int id, string name, string description, Conexión context)
    {

        // Ejecución
        try
        {

            var res = await (from I in context.DataBase.Inventarios
                             where I.ID == id
                             select I).ExecuteUpdateAsync(t => t.SetProperty(a => a.Nombre, name).SetProperty(a => a.Direction, description));


            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();

    }



    /// <summary>
    /// Obtiene un inventario.
    /// </summary>
    /// <param name="id">Id del producto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> FindByProduct(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = await (from p in context.DataBase.Productos
                             where p.Id == id
                             select p.InventoryId).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == 0)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene un inventario,
    /// </summary>
    /// <param name="id">Id del producto detalle</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> FindByProductDetail(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = await (from p in context.DataBase.ProductoDetalles
                             where p.Id == id
                             select p.Product.InventoryId).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == 0)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



}