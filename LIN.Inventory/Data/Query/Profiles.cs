namespace LIN.Inventory.Data.Query;


public class Profiles
{


    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<ProfileModel> Read(int id, Conexión context)
    {
        // Consulta
        var query = (from U in context.DataBase.Profiles
                     where U.ID == id
                     select U).Take(1);

        return query;

    }



}