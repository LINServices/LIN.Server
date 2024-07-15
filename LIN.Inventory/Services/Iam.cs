using LIN.Inventory.Data;

namespace LIN.Inventory.Services;


internal class Iam(Context context) : IIam
{


    /// <summary>
    /// Validar IAM.
    /// </summary>
    /// <param name="request">Solicitud.</param>
    public async Task<InventoryRoles> Validate(IamRequest request)
    {

        switch (request.IamBy)
        {
            case IamBy.Inventory:
                return await OnInventory(request.Id, request.Profile);

            case IamBy.Product:
                return await OnProduct(request.Id, request.Profile);

            case IamBy.Inflow:
                return await OnInflow(request.Id, request.Profile);

            case IamBy.Outflow:
                return await OnOutflow(request.Id, request.Profile);

            case IamBy.Access:
                return await OnAccess(request.Id, request.Profile);

            case IamBy.ProductDetail:
                break;
        }

        return InventoryRoles.Undefined;

    }










    /// <summary>
    /// Validar acceso.
    /// </summary>
    /// <param name="inventory">Id del inventario.</param>
    /// <param name="profile">Id del perfil.</param>
    private async Task<InventoryRoles> OnInventory(int inventory, int profile)
    {

        // Query.
        var access = await (from P in context.AccesoInventarios
                            where P.Inventario == inventory && P.ProfileID == profile
                            where P.State == InventoryAccessState.Accepted
                            select new { P.Rol }).FirstOrDefaultAsync();

        // Si no hay.
        if (access == null)
            return InventoryRoles.Undefined;

        return access.Rol;
    }




    private async Task<InventoryRoles> OnProduct(int id, int profile)
    {

        // Query.
        var access = await (from P in context.Productos
                            where P.Id == id
                            join AI in context.AccesoInventarios
                            on P.InventoryId equals AI.Inventario
                            where AI.State == InventoryAccessState.Accepted
                            where AI.ProfileID == profile
                            select new { AI.Rol }).FirstOrDefaultAsync();

        // Si no hay.
        if (access == null)
            return InventoryRoles.Undefined;

        return access.Rol;
    }


    private async Task<InventoryRoles> OnInflow(int id, int profile)
    {

        // Query.
        var access = await (from P in context.Entradas
                            where P.ID == id
                            join AI in context.AccesoInventarios
                            on P.InventoryId equals AI.Inventario
                            where AI.State == InventoryAccessState.Accepted
                            where AI.ProfileID == profile
                            select new { AI.Rol }).FirstOrDefaultAsync();

        // Si no hay.
        if (access == null)
            return InventoryRoles.Undefined;

        return access.Rol;
    }


    private async Task<InventoryRoles> OnOutflow(int id, int profile)
    {

        // Query.
        var access = await (from P in context.Salidas
                            where P.ID == id
                            join AI in context.AccesoInventarios
                            on P.InventoryId equals AI.Inventario
                            where AI.State == InventoryAccessState.Accepted
                            where AI.ProfileID == profile
                            select new { AI.Rol }).FirstOrDefaultAsync();

        // Si no hay.
        if (access == null)
            return InventoryRoles.Undefined;

        return access.Rol;
    }








    /// <summary>
    /// Validar acceso.
    /// </summary>
    /// <param name="id">Id del inventario.</param>
    /// <param name="profile">Id del perfil.</param>
    public async Task<bool> CanAccept(int id, int profile)
    {

        // Query.
        var access = await (from P in context.AccesoInventarios
                            where P.ID == id && P.ProfileID == profile
                            where P.State == InventoryAccessState.OnWait
                            select new { P.Rol }).FirstOrDefaultAsync();

        // Si no hay.
        return (access != null);

    }



    /// <summary>
    /// Validar acceso.
    /// </summary>
    /// <param name="accessId">Id del acceso.</param>
    /// <param name="profile">Id del perfil.</param>
    private async Task<InventoryRoles> OnAccess(int accessId, int profile)
    {

        // Query.
        var inventory = await (from P in context.AccesoInventarios
                               where P.ID == accessId
                               select P.Inventario).FirstOrDefaultAsync();

        // Rol.
        var rol = await OnInventory(inventory, profile);

        // Retornar.
        return rol;
    }


}