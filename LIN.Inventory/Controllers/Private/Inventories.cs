namespace LIN.Inventory.Controllers.Private;


internal class Inventories
{


    /// <summary>
    /// Valida si un usuario tiene acceso a un inventario
    /// </summary>
    /// <param name="inventory">ID</param>
    /// <param name="token">Token de acceso</param>
    public static async Task<ResponseBase> HaveAccess(int inventory, string token)
    {

        // Validación del JWT
        var (isValid, _, profile) = Jwt.Validate(token);

        if (!isValid)
            return new ResponseBase()
            {
                Message = "Token invalido",
                Response = Responses.Unauthorized
            };

        // Validación del parámetro
        if (inventory <= 0)
            return new ResponseBase()
            {
                Message = "ID del inventario es invalido",
                Response = Responses.InvalidParam
            };

        // Tiene acceso al proyecto
        var have = await Data.Inventories.HaveAuthorization(inventory, profile);

        // Si no tubo acceso
        if (have.Response != Responses.Success)
            return new ResponseBase()
            {
                Message = "No tienes acceso a este inventario",
                Response = Responses.Unauthorized
            };


        return new ResponseBase(Responses.Success);

    }




}
