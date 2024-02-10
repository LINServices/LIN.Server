namespace LIN.Inventory.Services;


public class Iam
{


    /// <summary>
    /// Validar acceso.
    /// </summary>
    /// <param name="inventory">Id del inventario.</param>
    /// <param name="profile">Id del perfil.</param>
    public static async Task<InventoryRoles> OnInventory(int inventory, int profile)
    {

        // Db.
        var (context, contextKey) = Conexión.GetOneConnection();

        // Query.
        var access = await(from P in context.DataBase.AccesoInventarios
                           where P.Inventario == inventory && P.ProfileID == profile
                           where P.State == InventoryAccessState.Accepted
                           select new { P.Rol }).FirstOrDefaultAsync();

        // Si no hay.
        if (access == null)
            return InventoryRoles.Undefined;

        return access.Rol;
    }



}