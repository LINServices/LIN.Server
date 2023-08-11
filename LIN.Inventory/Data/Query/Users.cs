namespace LIN.Server.Data.Query;


public class Users
{


    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<UserDataModel> Read(int id, Conexión context)
    {
        // Consulta
        var query = (from U in context.DataBase.Profiles
                     where U.ID == id
                     select U).Take(1);

        return query;

    }



    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="usuario">Usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<UserDataModel> Read(string usuario, Conexión context)
    {
        // Consulta
        var query = (from U in context.DataBase.Profiles
                     where U.Usuario == usuario
                     select U).Take(1);

        return query;

    }


}