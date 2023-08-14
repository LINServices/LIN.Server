using LIN.Inventory;
using LIN.Inventory.Data;
using LIN.Inventory.Services;

namespace LIN.Inventory.Controllers;


[Route("Inventory")]
public class InventoryController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo Inventario
    /// </summary>
    /// <param name="modelo">Modelo del inventario</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] InventoryDataModel modelo)
    {


        // Comprobaciones
        if (!modelo.UsersAccess.Any() || modelo.Creador <= 0 || !modelo.Nombre.Any() || !modelo.Direccion.Any())
            return new(Responses.InvalidParam);

        // Modelo
        foreach (var access in modelo.UsersAccess)
        {
            access.Fecha = DateTime.Now;
            if (modelo.Creador == access.ProfileID)
            {
                access.Rol = InventoryRoles.Administrator;
                access.State = InventoryAccessState.Accepted;
            }
            else
            {
                access.State = InventoryAccessState.OnWait;
            }
        }


        // Crea el inventario
        var response = await Inventories.Create(modelo);

        // Si no se creo el inventario
        if (response.Response != Responses.Success)
            return response;


        // Retorna
        return response;

    }



    /// <summary>
    /// Obtiene los inventarios asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<InventoryDataModel>> ReadAll([FromHeader] int id)
    {

        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene la lista de ID's de inventarios
        var result = await Inventories.ReadAll(id);

        return result;

    }



    /// <summary>
    /// Actualiza el rol de un usuario en un inventario
    /// </summary>
    /// <param name="accessID">ID del acceso</param>
    /// <param name="newRol">Nuevo rol</param>
    /// <param name="token">Token de acceso</param>
    [HttpPatch("update/rol")]
    public async Task<HttpResponseBase> UpdateRol([FromHeader] int accessID, [FromHeader] InventoryRoles newRol, [FromHeader] string token)
    {

        // Comprobaciones
        if (accessID <= 0)
            return new(Responses.InvalidParam);


        var (isValid, _, userId) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.Unauthorized);




        var response = await InventoryAccess.UpdateRol(accessID, userId, newRol);

        // Retorna
        return response;

    }



    /// <summary>
    /// Estadísticas del home
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="days">Cantidad de días atrás</param>
    [HttpGet("home")]
    public async Task<HttpReadOneResponse<HomeDto>> HomeService([FromHeader] int id, [FromHeader] int days)
    {

        if (id <= 0 || days < 0)
            return new(Responses.InvalidParam);



        var (context, contextKey) = Conexión.GetOneConnection();

        var ventas30 = await Outflows.VentasOf(id, 30, context);
        var ventas7 = await Outflows.VentasOf(id, 7, context);

        var compras30 = await Inflows.ComprasOf(id, 30, context);
        var compras7 = await Inflows.ComprasOf(id, 7, context);

        context.CloseActions(contextKey);

        return new ReadOneResponse<HomeDto>()
        {
            Response = Responses.Success,
            Model = new()
            {
                Compras30 = compras30.Model,
                Compras7 = compras7.Model,
                Ventas30 = ventas30.Model,
                Ventas7 = ventas7.Model
            }
        };
    }



    /// <summary>
    /// Obtiene la valuación de los inventarios donde un usuario es administrador
    /// </summary>
    /// <param name="id">ID del usuario</param>
    [HttpGet("valuation")]
    public async Task<HttpReadOneResponse<decimal>> Valuation([FromHeader] int id)
    {
        if (id <= 0)
            return new(Responses.InvalidParam);

        return await Inventories.ValueOf(id);
    }



}