namespace LIN.Inventory.Data.Query;

public class Profiles
{


    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<ProfileModel> Read(int id, Context context)
    {
        // Consulta
        var query = (from U in context.Profiles
                     where U.Id == id
                     select U).Take(1);

        return query;

    }


    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<ProfileModel> Read(List<int> id, Context context)
    {
        // Consulta
        var query = (from U in context.Profiles
                     where id.Contains(U.Id)
                     select U);

        return query;

    }


    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<ProfileModel> ReadByAccounts(List<int> id, Context context)
    {
        // Consulta
        var query = (from U in context.Profiles
                     where id.Contains(U.AccountId)
                     select U);

        return query;

    }


    /// <summary>
    /// Obtener usuarios
    /// </summary>
    /// <param name="id">Id del usuario</param>
    /// <param name="context">Contexto</param>
    public static IQueryable<ProfileModel> ReadByAccount(int id, Context context)
    {
        // Consulta
        var query = (from U in context.Profiles
                     where U.AccountId == id
                     select U).Take(1);

        return query;

    }

}