using LIN.Inventory.Services;

namespace LIN.Inventory.Data;


public class Inventories
{


    #region Abstracciones


    /// <summary>
    /// Crea un nuevo inventario
    /// </summary>
    /// <param name="data">Modelo del inventario</param>
    public async static Task<CreateResponse> Create(InventoryDataModel data)
    {

        // Conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Create(data, context);
        context.CloseActions(connectionKey);
        return response;
    }



    /// <summary>
    /// Obtiene un inventario por medio del ID
    /// </summary>
    /// <param name="id">ID del inventario</param>
    public async static Task<ReadOneResponse<InventoryDataModel>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Read(id, context);
        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene la lista de inventarios asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadAllResponse<InventoryDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return response;

    }



    /// <summary>
    /// Obtiene la valuación de los inventarios donde un usuario es admin
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadOneResponse<decimal>> ValueOf(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await ValueOf(id, context);
        context.CloseActions(connectionKey);
        return response;
    }



    #endregion



    /// <summary>
    /// Crea un nuevo inventario
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

                // Inventario
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
    /// Obtiene un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
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
    /// Obtiene la lista de inventarios asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<InventoryDataModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = from AI in context.DataBase.AccesoInventarios
                      where AI.Usuario == id && AI.State == InventoryAccessState.Accepted
                      join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                      select new InventoryDataModel()
                      {
                          MyRol = AI.Rol,
                          Creador = I.Creador,
                          Direccion = I.Direccion,
                          ID = I.ID,
                          Nombre = I.Nombre,
                          UltimaModificacion = I.UltimaModificacion,
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
    /// Obtiene la valuación de los inventarios donde un usuario es admin
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<decimal>> ValueOf(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Selecciona la entrada
            var query = from AI in context.DataBase.AccesoInventarios
                        where AI.Usuario == id && AI.State == InventoryAccessState.Accepted && AI.Rol == InventoryRols.Administrator
                        join I in context.DataBase.Inventarios on AI.Inventario equals I.ID
                        join P in context.DataBase.Productos on I.ID equals P.Inventory
                        join PD in context.DataBase.ProductoDetalles on P.ID equals PD.ProductoFK
                        where PD.Estado == ProductStatements.Normal
                        select PD.PrecioVenta * PD.Quantity;


            var valor = await query.SumAsync();

            // Retorna
            return new(Responses.Success, valor);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



}